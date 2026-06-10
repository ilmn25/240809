using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;
using Debug = UnityEngine.Debug;

[Serializable]
public enum GenType
{
    Abyss, SkyBlock, SuperFlat, Backrooms
}
public class Gen
{
    protected virtual void GenChunk(Vector3Int currentCoordinate, Chunk currentChunk) { }
    public virtual Vector3Int GetSize() => Vector3Int.one;
    public virtual Vector3Int GetSpawnPoint() => Vector3Int.zero;
    
    protected static System.Random Random = new (Save.Inst?.seed ?? 0);
    protected static Gen _target;

    public static readonly Dictionary<GenType, Gen> Dictionary = new ()
    {
        {GenType.Abyss, new GenAbyss()},
        {GenType.SkyBlock, new GenSkyBlock()},
        {GenType.SuperFlat, new GenSuperFlat()},
        {GenType.Backrooms, new GenBackrooms()},
    };
    public static void Initialize(GenType genType)
    {
        Random = new System.Random(Save.Inst.seed);
        _target = Dictionary[genType];
    }
    public static float GetOffset()
    {
        return (float)Random.NextDouble() * 1000f;
    }

    /// <summary>
    /// Returns a deterministic Perlin-noise offset derived from the world seed and
    /// a unique salt string.  Unlike GetOffset(), this does NOT consume the shared
    /// Random state, so it is safe to use in static field initializers without
    /// introducing non-determinism.
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

    public static IEnumerator GenerateNearbyChunks(Vector3Int center, int range)
    {
        Vector3Int position; 
        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                for (int z = -range; z <= range; z++)
                {
                    position = new Vector3Int(
                        center.x + x * World.ChunkSize,
                        center.y + y * World.ChunkSize,
                        center.z + z * World.ChunkSize);

                    if (World.Inst[position] == null)
                    {
                        Generate(position);
                        yield return null;  
                    }
                }
            }
        }
    }

    public static void Generate(Vector3Int currentCoordinate)
    {
        Chunk currentChunk = new Chunk();
        World.Inst[currentCoordinate] = currentChunk;
        _target.GenChunk(currentCoordinate, currentChunk);
    }   
}
