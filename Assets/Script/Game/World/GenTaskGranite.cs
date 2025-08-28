using UnityEngine;

public class GenTaskGranite : Gen
{
    private const float Scale = 0.3f;
    private const float Threshold = 0.5f;
    private const float MaxGraniteHeight = 180;

    private static readonly float Offset = GetOffset();

    private static int _idGranite;
    private static int ID => _idGranite == 0 ? Block.ConvertID(global::ID.GraniteBlock) : _idGranite;

    public static void Run(Vector3Int currentCoordinate, Chunk currentChunk)
    {
        int chunkSize = World.ChunkSize;

        for (int x = 0; x < chunkSize; x++)
        {
            int worldX = currentCoordinate.x + x;

            for (int z = 0; z < chunkSize; z++)
            {
                int worldZ = currentCoordinate.z + z;

                for (int y = 0; y < chunkSize; y++)
                {
                    int worldY = currentCoordinate.y + y;

                    // Only generate granite below a certain height
                    if (worldY > MaxGraniteHeight) continue;

                    // Get Perlin3D noise value
                    float nx = worldX * Scale + Offset;
                    float ny = worldY * Scale + Offset;
                    float nz = worldZ * Scale + Offset;

                    float noise = Perlin3D(nx, ny, nz);

                    // If noise is above threshold, place granite
                    if (noise > Threshold && currentChunk[x, y, z] != 0)
                    {
                        currentChunk[x, y, z] = ID;
                    }
                }
            }
        }
    } 
    private static float Perlin3D(float x, float y, float z)
    {
        float xy = Mathf.PerlinNoise(x, y);
        float yz = Mathf.PerlinNoise(y, z);
        float xz = Mathf.PerlinNoise(x, z);
        float yx = Mathf.PerlinNoise(y, x);
        float zy = Mathf.PerlinNoise(z, y);
        float zx = Mathf.PerlinNoise(z, x);

        return (xy + yz + xz + yx + zy + zx) / 6f;
    }
}
