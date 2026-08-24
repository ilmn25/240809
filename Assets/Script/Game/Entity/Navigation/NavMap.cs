using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Occupancy map used by pathfinding and movement. Each cell holds a value:
/// Air (0) = empty, Door (1) = blocks movement but pathfinding can route
/// through (with a cost penalty), Block (2) = fully solid, Semi (3) = fully
/// solid like Block (slab, sign, bed, ...) — cannot be walked on or passed through.
/// </summary>
public class NavMap
{
    public const byte Air = 0;   // empty, walkable
    public const byte Door = 1;  // closed door — blocks movement, pathfinding can route through
    public const byte Block = 2; // fully solid
    public const byte Semi = 3;  // fully solid (slab, sign, bed, ...) — same as Block

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

    /// <summary>True if any solid block occupies the given box footprint — the
    /// standard mob/entity collider shape. Shared by MovementModule and any
    /// entity that moves itself manually.</summary>
    public static bool IsBlocked(Vector3 position, float halfX = 0.35f, float halfZ = 0.25f, float height = 0.7f)
    {
        int minX = Mathf.FloorToInt(position.x - halfX);
        int maxX = Mathf.FloorToInt(position.x + halfX);
        int minZ = Mathf.FloorToInt(position.z - halfZ);
        int maxZ = Mathf.FloorToInt(position.z + halfZ);
        int minY = Mathf.FloorToInt(position.y);
        int maxY = Mathf.FloorToInt(position.y + height);

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                for (int z = minZ; z <= maxZ; z++)
                {
                    Vector3Int b = new Vector3Int(x, y, z);
                    if (World.IsInWorldBounds(b) && Get(b) != Air)
                        return true;
                }
            }
        }
        return false;
    }

    /// <summary>True if the cell is navigable for pathfinding (air or door).</summary>
    public static bool IsNavigable(Vector3Int worldPosition)
    {
        byte value = Get(worldPosition);
        return value == Air || value == Door;
    }

    /// <summary>True if a cell is passable but pathfinding should avoid it when possible.</summary>
    public static bool IsSemiBlocking(byte value) => value == Door;

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
        if (_map == null || (entity.Collision != Main.IndexCollide && entity.Collision != Main.IndexSemiCollide)) return;
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
