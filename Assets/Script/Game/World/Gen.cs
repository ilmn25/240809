using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum GenType
{
    Abyss, SkyBlock, SuperFlat, Backrooms, Dungeon, Edit
}
/// <summary>A single generation step. Implement RunChunk for per-chunk block
/// work; RunWorld for work once, after all chunks are generated.</summary>
public interface IGenTask
{
    void RunChunk(Vector3Int coord, Chunk chunk) { }
    void RunWorld(World world) { }
}

public abstract class Gen
{
    // Ordered pipeline of tasks. Worlds with bespoke generation override
    // GenChunk/GenPostWorld directly instead.
    protected virtual IGenTask[] Tasks => Array.Empty<IGenTask>();

    protected virtual void GenChunk(Vector3Int currentCoordinate, Chunk currentChunk)
    {
        foreach (IGenTask task in Tasks) task.RunChunk(currentCoordinate, currentChunk);
    }

    protected virtual void GenPostWorld(World world)
    {
        foreach (IGenTask task in Tasks) task.RunWorld(world);
    }

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

        // Per-world-type post-processing (scatter tasks, setpieces, entity spawn).
        gen.GenPostWorld(world);
    }

    /// <summary>Coroutine version of <see cref="GenerateAllFor"/> that yields every
    /// few chunks so generation runs over frames (e.g. during the intro dialogue).</summary>
    public static IEnumerator GenerateAllForCoroutine(World world, Action<float> onProgress = null)
    {
        // Skip if already generated (loaded from save)
        if (world[Vector3Int.zero] != null && world[Vector3Int.zero] != Chunk.Zero)
        {
            onProgress?.Invoke(1f);
            yield break;
        }

        Gen gen = Dictionary[world.GenType];
        int chunkSize = World.ChunkSize;
        int total = world.Size.x * world.Size.y * world.Size.z;
        int count = 0;
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
                    count++;
                    onProgress?.Invoke((float)count / total);
                    if ((count & 7) == 0) yield return null;
                }
            }
        }

        // Per-world-type post-processing (scatter tasks, setpieces, entity spawn).
        gen.GenPostWorld(world);
    }

}
