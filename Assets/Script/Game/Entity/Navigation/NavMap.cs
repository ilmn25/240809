using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// Occupancy map used by pathfinding and movement. Each cell holds a value:
/// Air (0.0, byte 0) = empty, Door (0.5, byte 1) = blocks movement but is still
/// navigable by pathfinding, Block (1.0, byte 2) = fully solid.
/// </summary>
public class NavMap
{
    public const byte Air = 0;   // 0.0 — empty, walkable
    public const byte Door = 1;  // 0.5 — blocks movement, pathfinding can route through
    public const byte Block = 2; // 1.0 — fully solid

    private static byte[] _map;
    private static readonly List<Vector3Int> LoadedChunks = new ();

    public static void Initialize()
    {
        _map = new byte[
            World.Inst.Bounds.x * World.Inst.Bounds.y * World.Inst.Bounds.z];
        LoadedChunks.Clear();
    }

    private static int GetIndex(int x, int y, int z)
    {
        return x + World.Inst.Bounds.x * (y + World.Inst.Bounds.y * z);
    }
    private static int GetIndex(Vector3Int coordinate)
    {
        return coordinate.x + World.Inst.Bounds.x * (coordinate.y + World.Inst.Bounds.y * coordinate.z);
    }

    /// <summary>Process a chunk into NavMap. Returns true if new data was loaded (false if already done).</summary>
    public static bool SetChunk(Vector3Int coordinate)
    {
        if (!World.IsInWorldBounds(coordinate) || LoadedChunks.Contains(coordinate)) return false;
        LoadedChunks.Add(coordinate);
        Chunk chunk = World.Inst[coordinate.x, coordinate.y, coordinate.z];
        if (chunk != null)
        {
            for (int x = 0; x < World.ChunkSize; x++)
            {
                for (int y = 0; y < World.ChunkSize; y++)
                {
                    for (int z = 0; z < World.ChunkSize; z++)
                    {
                        Set(coordinate.x + x, coordinate.y + y, coordinate.z + z, chunk[x, y, z] == 0 ? Air : Block);
                    }
                }
            }
            foreach (var entity in chunk.StaticEntity)
            {
                SetEntity(Entity.Dictionary[entity.id], entity.position, Entity.Dictionary[entity.id].NavValue);
            }
        }
        return true;
    }

    public static Vector3Int GetRelativePosition(Vector3Int coordinate)
    {
        return coordinate;
    }

    /// <summary>The NavMap value at a cell: Air (0), Door (1), or Block (2).</summary>
    public static byte Get(Vector3Int worldPosition)
    {
        if (_map == null) return Block;
        if (!World.IsInWorldBounds(worldPosition)) return Air;
        return _map[GetIndex(worldPosition)];
    }

    /// <summary>True if the cell is fully empty (walkable, no door).</summary>
    public static bool IsAir(Vector3Int worldPosition) => Get(worldPosition) == Air;

    /// <summary>True if the cell is navigable for pathfinding (air or door).</summary>
    public static bool IsNavigable(Vector3Int worldPosition) => Get(worldPosition) != Block;

    public static void Set(Vector3Int worldPosition, byte value)
    {
        if (_map == null || !World.IsInWorldBounds(worldPosition)) return;
        _map[GetIndex(worldPosition)] = value;
    }

    public static void Set(int x, int y, int z, byte value)
    {
        if (_map == null || !World.IsInWorldBounds(x, y, z)) return;
        _map[GetIndex(x, y, z)] = value;
    }

    /// <summary>
    /// Marks every cell a static entity occupies with the given NavMap value.
    /// Pass Air to clear (entity removed), or the entity's own NavValue to block.
    /// </summary>
    public static void SetEntity(Entity entity, Vector3 position, byte value)
    {
        if (_map == null || entity.Collision != Main.IndexCollide) return;
        int entityX = Mathf.FloorToInt(position.x);
        int entityY = Mathf.FloorToInt(position.y);
        int entityZ = Mathf.FloorToInt(position.z);

        Vector3Int bounds = Vector3Int.FloorToInt(entity.Bounds);
        int entityEndX = entityX + bounds.x;
        int entityEndY = entityY + bounds.y;
        int entityEndZ = entityZ + bounds.z;

        for (int x = entityX; x < entityEndX; x++)
        {
            for (int y = entityY; y < entityEndY; y++)
            {
                for (int z = entityZ; z < entityEndZ; z++)
                {
                    NavMap.Set(x, y, z, value);
                }
            }
        }
    }
}
