using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum GenType
{
    // 1 (SkyBlock) was removed together with Set/SkyBlock.json. Values are explicit so
    // saves written before the removal keep resolving to the same worlds.
    Abyss = 0, SuperFlat = 2, Backrooms = 3, Dungeon = 4, Edit = 5, Maw = 6
}
/// <summary>A single generation step. RunChunk for per-chunk block work; RunWorld for work once, after all chunks are generated.</summary>
public interface IGenTask
{
    void RunChunk(Vector3Int coord, Chunk chunk) { }
    void RunWorld(World world) { }
}

public abstract class Gen
{
    // Ordered pipeline of tasks. Bespoke worlds override GenChunk/GenPostWorld instead.
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
        {GenType.SuperFlat, new GenSuperFlat()},
        {GenType.Backrooms, new GenBackrooms()},
        {GenType.Dungeon, new GenDungeon()},
        {GenType.Edit, new GenEdit()},
        {GenType.Maw, new GenMaw()},
    };

    /// <summary>Deterministic Perlin-noise offset from world seed + salt; doesn't consume shared Random state.</summary>
    public static float GetDeterministicOffset(string salt)
    {
        int hash = CombineHashes(Save.Inst.seed, DeterministicStringHash(salt));
        return (float)(new System.Random(hash).NextDouble()) * 1000f;
    }

    /// <summary>System.Random seeded from world seed + salt + chunk coordinate (stable regardless of execution order).</summary>
    public static System.Random CreateChunkRandom(string salt, Vector3Int chunkCoord)
    {
        int hash = CombineHashes(Save.Inst.seed, 
                   CombineHashes(DeterministicStringHash(salt),
                   CombineHashes(chunkCoord.x, 
                   CombineHashes(chunkCoord.y, chunkCoord.z))));
        return new System.Random(hash);
    }

    /// <summary>Whole-world System.Random from world seed + salt (full entropy; avoid (int)GetDeterministicOffset seeds).</summary>
    public static System.Random CreateWorldRandom(string salt)
    {
        int hash = CombineHashes(Save.Inst.seed, DeterministicStringHash(salt));
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

    /// <summary>Stable string hash identical on all .NET runtimes (unlike string.GetHashCode()).</summary>
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

    /// <summary>Generates every chunk up-front (Terraria-style); skips already-generated worlds; caller handles NavMap.</summary>
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

    /// <summary>Coroutine version of <see cref="GenerateAllFor"/> that yields every few chunks to spread over frames.</summary>
    public static IEnumerator GenerateAllForCoroutine(World world)
    {
        // Skip if already generated (loaded from save)
        if (world[Vector3Int.zero] != null && world[Vector3Int.zero] != Chunk.Zero)
            yield break;

        Gen gen = Dictionary[world.GenType];
        int chunkSize = World.ChunkSize;
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
                    if ((count & 50) == 0) yield return null;
                }
            }
        }

        // Per-world-type post-processing (scatter tasks, setpieces, entity spawn).
        gen.GenPostWorld(world);
    }

}
