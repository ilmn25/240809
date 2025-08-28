using UnityEngine;

public class GenTaskCaves : Gen
{
    private const float Scale = 0.03f;
    private static readonly float Offset = GetOffset(); 
    public static void Run(Vector3Int currentCoordinate, Chunk currentChunk)
    {
        for (int x = 0; x < World.ChunkSize; x++)
        {
            float caveX = (currentCoordinate.x + x) * Scale + Offset;
            for (int z = 0; z < World.ChunkSize; z++)
            {
                float caveZ = (currentCoordinate.z + z) * Scale + Offset;
                for (int y = 0; y < World.ChunkSize; y++)
                {
                    float caveY = (currentCoordinate.y + y) * Scale + Offset; 
                    float caveNoiseValue = Mathf.PerlinNoise(caveX, caveY) * Mathf.PerlinNoise(caveY, caveZ);

                    if (caveNoiseValue > 0.35f)
                    {
                        currentChunk[x, y, z] = 0; // Empty space for caves
                    }
                    
                    // Convert local chunk coordinates to world position
                    // Vector3 position = new Vector3(CurrentCoordinate.x + x, CurrentCoordinate.y + y, CurrentCoordinate.z + z);
                    //
                    // // If density is below threshold, it's a cave (air)
                    // if (_densityGen.GetDensity(position) < _densityGen.threshold)
                    // {
                    //     CurrentChunk[x, y, z] = 0; // Empty space for caves
                    // }  
                }
            }
        }
    } 
}
