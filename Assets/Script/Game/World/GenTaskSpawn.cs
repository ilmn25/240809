using UnityEngine;

public class GenTaskSpawn : Gen
{
    public static void Run(Vector3Int currentCoordinate, Chunk currentChunk)
    {
        // only operate on the chunk that contains the configured spawn point
        Vector3Int spawnWorld = _target.SpawnPoint;
        Vector3Int spawnChunk = World.GetChunkCoordinate(spawnWorld);
        if (spawnChunk != currentCoordinate) return;

        // compute surface y at the spawn location
        int surfaceY = FindSurfaceY(spawnWorld.x, spawnWorld.z, currentCoordinate);
        Vector3Int spawnPos = new Vector3Int(spawnWorld.x, surfaceY, spawnWorld.z);
        World.Inst.SpawnPoint = spawnPos;

    }

    // copied logic from GenTaskWall to locate the first air block above solid ground
    private static int FindSurfaceY(int worldX, int worldZ, Vector3Int currentChunk)
    {
        int surfaceY = -1;
        Vector3Int scanChunk = currentChunk;
        int localX = worldX - currentChunk.x;
        int localZ = worldZ - currentChunk.z;

        // look upward first
        while (scanChunk.y <= _target.WorldHeight)
        {
            Chunk scan = World.Inst[scanChunk];
            if (scan != null)
            {
                for (int ly = 0; ly < World.ChunkSize; ly++)
                {
                    if (scan[localX, ly, localZ] != 0)
                    {
                        surfaceY = scanChunk.y + ly + 1;
                        break;
                    }
                }
                if (surfaceY >= 0)
                    break;
            }
            scanChunk.y += World.ChunkSize;
        }

        // if nothing above, search downward
        if (surfaceY < 0)
        {
            scanChunk = currentChunk;
            while (scanChunk.y >= 0)
            {
                Chunk scan = World.Inst[scanChunk];
                if (scan != null)
                {
                    for (int ly = World.ChunkSize - 1; ly >= 0; ly--)
                    {
                        if (scan[localX, ly, localZ] != 0)
                        {
                            surfaceY = scanChunk.y + ly + 1;
                            break;
                        }
                    }
                    if (surfaceY >= 0)
                        break;
                }
                scanChunk.y -= World.ChunkSize;
            }
        }

        if (surfaceY < 0)
        {
            // fallback to bottom of this chunk
            surfaceY = currentChunk.y;
        }

        return surfaceY;
    }
}
