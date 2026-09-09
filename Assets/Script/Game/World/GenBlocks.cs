using UnityEngine;

/// <summary>Low-level block/entity writes shared by the world generators and set
/// piece pasting. These write straight to chunk data — no NavMap sync or map
/// refresh — because they run during generation, before the live world exists.
/// (For runtime edits use World.SetBlock, which keeps NavMap in sync.)</summary>
public static class GenBlocks
{
    /// <summary>The chunk containing a world position, or null when the position
    /// is out of bounds or its chunk isn't created.</summary>
    public static Chunk ChunkAt(World world, Vector3Int pos)
    {
        Chunk chunk = world[World.GetChunkCoordinate(pos)];
        return chunk == null || chunk == Chunk.Zero ? null : chunk;
    }

    /// <summary>Sets a raw block at a world position (no navmap/sync/refresh).</summary>
    public static void SetBlock(World world, Vector3Int pos, int blockID)
    {
        if (pos.x < 0 || pos.x >= world.Bounds.x ||
            pos.y < 0 || pos.y >= world.Bounds.y ||
            pos.z < 0 || pos.z >= world.Bounds.z) return;
        Chunk chunk = world[pos];
        if (chunk == null || chunk == Chunk.Zero) return;
        chunk[World.GetBlockCoordinate(pos)] = blockID;
    }

    public static void SetBlock(World world, int x, int y, int z, int blockID)
        => SetBlock(world, new Vector3Int(x, y, z), blockID);

    /// <summary>First air block directly above a solid block in the column, or -1.</summary>
    public static int FindSurfaceY(World world, int x, int z)
    {
        for (int y = world.Bounds.y - 1; y >= 1; y--)
        {
            Vector3Int block = new Vector3Int(x, y, z);
            Vector3Int chunkCoord = World.GetChunkCoordinate(block);
            Chunk chunk = world[chunkCoord];
            if (chunk == null || chunk == Chunk.Zero) continue;

            int localX = block.x - chunkCoord.x;
            int localY = block.y - chunkCoord.y;
            int localZ = block.z - chunkCoord.z;
            if (localY == 0) continue;

            if (chunk[localX, localY, localZ] == 0 && chunk[localX, localY - 1, localZ] != 0)
                return y;
        }
        return -1;
    }

    /// <summary>Adds a scatter entity to its chunk: structures go to StaticEntity
    /// (persist + show on the map), loose items to DynamicEntity (spawn as items).</summary>
    public static void PlaceEntity(World world, Vector3Int cell, ID id)
    {
        if (cell.y < 0) return; // no valid surface
        Info info = Entity.CreateInfo(id, cell);
        if (info == null) return;
        Chunk chunk = ChunkAt(world, cell);
        if (chunk == null) return;
        if (Entity.Dictionary.ContainsKey(id))
            chunk.StaticEntity.Add(info);
        else
            chunk.DynamicEntity.Add(info);
    }
}
