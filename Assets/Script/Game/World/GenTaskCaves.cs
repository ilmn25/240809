using UnityEngine;

public class GenTaskCaves : WorldGen
{
    private const float Scale = 0.03f;
    private static readonly float Offset = GetOffset();
    private static DensityGenerator _densityGen = new DensityGenerator
    {
        noiseScale = 0.1f,
        threshold = 0.4f // You can tweak this for cave density
    };
    public static void Run(Vector3Int CurrentCoordinate, Chunk CurrentChunk)
    {
        for (int x = 0; x < World.ChunkSize; x++)
        {
            float caveX = (CurrentCoordinate.x + x) * Scale + Offset;
            for (int z = 0; z < World.ChunkSize; z++)
            {
                float caveZ = (CurrentCoordinate.z + z) * Scale + Offset;
                for (int y = 0; y < World.ChunkSize; y++)
                {
                    float caveY = (CurrentCoordinate.y + y) * Scale + Offset; 
                    float caveNoiseValue = Mathf.PerlinNoise(caveX, caveY) * Mathf.PerlinNoise(caveY, caveZ);

                    if (caveNoiseValue > 0.35f)
                    {
                        CurrentChunk[x, y, z] = 0; // Empty space for caves
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
