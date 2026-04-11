using UnityEngine;

public class GenTaskWall : Gen
{
    private static int _id;
    private static int Brick => _id == 0 ? Block.ConvertID(ID.BrickBlock) : _id;
    private const int Thickness = 2;
    private const int RadiusChunks = 2;
    // how far the wall extends relative to spawn point
    private const int AboveSpawn = 5;
    private const int BelowSpawn = 4;

    public static void Run(Vector3Int currentCoordinate, Chunk currentChunk)
    {
        Vector3Int center = World.Inst.SpawnPoint;
        // no vertical filtering; wall will be placed relative to surface height below

        for (int x = 0; x < World.ChunkSize; x++)
        {
            int globalX = currentCoordinate.x + x;
            int dx = globalX - center.x;

            for (int z = 0; z < World.ChunkSize; z++)
            {
                int globalZ = currentCoordinate.z + z;
                int dz = globalZ - center.z;

                // circular ring test
                int rMin = RadiusChunks * World.ChunkSize;
                int rMax = rMin + Thickness;
                int distSq = dx * dx + dz * dz;
                // include any block whose distance falls within the annulus [rMin, rMax]
                if (distSq < rMin * rMin || distSq > rMax * rMax)
                    continue;

                // determine surface height by scanning existing chunks up and then down
                int surfaceY = -1;
                Vector3Int scanChunk = currentCoordinate;
                int localX = x;
                int localZ = z;

                // first look upward for any solid block
                while (scanChunk.y <= (World.Inst.Size.y - 3) * World.ChunkSize)
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

                // if nothing above, look down through lower chunks
                if (surfaceY < 0)
                {
                    scanChunk = currentCoordinate;
                    while (scanChunk.y >= 0)
                    {
                        Chunk scan = World.Inst[scanChunk];
                        if (scan != null)
                        {
                            for (int ly = World.ChunkSize - 1; ly >= 0; ly--)
                            {
                                if (scan[localX, ly, localZ] != 0)
                                {
                                    surfaceY = scanChunk.y + ly + 1; // first air above solid
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
                    // still nothing found; default to bottom of current chunk
                    surfaceY = currentCoordinate.y;
                }

                int minY = surfaceY - BelowSpawn;
                int maxY = surfaceY + AboveSpawn;

                // write blocks across all affected chunks vertically
                for (int worldY = minY; worldY <= maxY; worldY++)
                {
                    Vector3Int worldPos = new Vector3Int(globalX, worldY, globalZ);
                    Vector3Int chunkPos = World.GetChunkCoordinate(worldPos);
                    Vector3Int blockPos = World.GetBlockCoordinate(worldPos);

                    Chunk targetChunk = World.Inst[chunkPos];
                    if (targetChunk == null) 
                        Gen.Generate(chunkPos);

                    int existingID = World.Inst[chunkPos][blockPos.x, blockPos.y, blockPos.z];
                    if (existingID == 0)
                        World.Inst[chunkPos][blockPos.x, blockPos.y, blockPos.z] = Brick;
                }
                continue;
            }
        }
    }
}
