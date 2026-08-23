using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum GenType
{
    Abyss, SkyBlock, SuperFlat, Backrooms, Dungeon, Edit
}
public class Gen
{
    protected virtual void GenChunk(Vector3Int currentCoordinate, Chunk currentChunk) { }
    protected virtual void GenPostWorld(World world) { }
    public virtual Vector3Int GetSize() => Vector3Int.one;
    public virtual Vector3Int GetSpawnPoint() => Vector3Int.zero;
    
    public static readonly Dictionary<GenType, Gen> Dictionary = new ()
    {
        {GenType.Abyss, new GenAbyss()},
        {GenType.SkyBlock, new GenSkyBlock()},
        {GenType.SuperFlat, new GenSuperFlat()},
        {GenType.Backrooms, new GenBackrooms()},
        {GenType.Dungeon, new GenDungeon()},
        {GenType.Edit, new GenEdit()},
    };

    /// <summary>
    /// Returns a deterministic Perlin-noise offset derived from the world seed and
    /// a unique salt string.  This does NOT consume any shared Random state,
    /// so it is safe to use in static field initializers without introducing
    /// non-determinism.
    /// </summary>
    public static float GetDeterministicOffset(string salt)
    {
        int hash = CombineHashes(Save.Inst.seed, DeterministicStringHash(salt));
        return (float)(new System.Random(hash).NextDouble()) * 1000f;
    }

    /// <summary>
    /// Creates a System.Random seeded from the world seed + salt + chunk coordinate.
    /// This ensures every chunk+task combination always gets the same random sequence,
    /// regardless of execution order.
    /// </summary>
    public static System.Random CreateChunkRandom(string salt, Vector3Int chunkCoord)
    {
        int hash = CombineHashes(Save.Inst.seed, 
                   CombineHashes(DeterministicStringHash(salt),
                   CombineHashes(chunkCoord.x, 
                   CombineHashes(chunkCoord.y, chunkCoord.z))));
        return new System.Random(hash);
    }

    private static int CombineHashes(int h1, int h2)
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + h1;
            hash = hash * 31 + h2;
            return hash;
        }
    }

    /// <summary>
    /// A stable string hash that produces the same value on all .NET
    /// runtimes and platforms (unlike string.GetHashCode()).
    /// </summary>
    private static int DeterministicStringHash(string str)
    {
        unchecked
        {
            int hash = 5381;
            for (int i = 0; i < str.Length; i++)
                hash = hash * 33 + str[i];
            return hash;
        }
    }

    /// <summary>
    /// Generates every chunk for the given world up-front (Terraria-style).
    /// Skips worlds that already have data (e.g. loaded from a save file).
    /// Does NOT write into NavMap — the caller is responsible for that.
    /// </summary>
    public static void GenerateAllFor(World world)
    {
        // Skip if already generated (loaded from save)
        if (world[Vector3Int.zero] != null && world[Vector3Int.zero] != Chunk.Zero)
            return;

        Gen gen = Dictionary[world.GenType];
        int chunkSize = World.ChunkSize;
        for (int cx = 0; cx < world.Size.x; cx++)
        {
            for (int cy = 0; cy < world.Size.y; cy++)
            {
                for (int cz = 0; cz < world.Size.z; cz++)
                {
                    Vector3Int coord = new Vector3Int(cx * chunkSize, cy * chunkSize, cz * chunkSize);
                    Chunk chunk = new Chunk();
                    world[coord] = chunk;
                    gen.GenChunk(coord, chunk);
                }
            }
        }

        // Per-world-type post-processing (e.g. Abyss spawn statue).
        gen.GenPostWorld(world);

        // Spawn surface entities AFTER all block tasks so they sit on final terrain.
        // Only the Abyss dimension uses natural surface entity spawning.
        if (world.GenType == GenType.Abyss)
            GenTaskEntity.RunAll(world);

        // Scatter a graveyard cluster in the grass biome.
        GenTaskGraveyard.Run(world);

        // Place raider camps (loot chest + tents + lamp).
        GenTaskRaiderCamp.Run(world);

        // Scatter a few ponds (each ringed with bushes).
        GenTaskPond.Run(world);

        // Place a fortified outpost watchtower.
        GenOutpost.Run(world);

        // Place a brick stairwell entrance to the dungeon.
        GenDungeonEntrance.Run(world);
    }

}
