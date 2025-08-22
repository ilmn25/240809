using UnityEngine;

public class GenTaskCrater : WorldGen
{
    private static readonly int centerX = World.Inst.Bounds.x/2;
    private static readonly int centerZ = World.Inst.Bounds.z/2;
    private static readonly int Radius = World.Inst.Bounds.z/5;
    private const float Scale = 0.1f;
    private const int Steps = 5;
    private static readonly float Offset = GetOffset();
    private static int _id;
    private static int ID => _id == 0 ? Block.ConvertID(global::ID.DirtBlock) : _id; 

    public static void Run()
    {
        int chunkSize = World.ChunkSize;

        for (int x = 0; x < chunkSize; x++)
        {
            int worldX = CurrentCoordinate.x + x;
            int distanceX = Mathf.Abs(centerX - worldX);
            float noiseX = worldX * Scale + Offset;

            for (int z = 0; z < chunkSize; z++)
            {
                int worldZ = CurrentCoordinate.z + z;
                int distanceZ = Mathf.Abs(centerZ - worldZ);
                float noiseZ = worldZ * Scale + Offset;

                float distanceSquared = distanceX * distanceX + distanceZ * distanceZ;
                float wallNoise = (Mathf.PerlinNoise(noiseX, noiseZ) * 2f - 1f) * 5f;
                float spatialOffset = (Mathf.PerlinNoise(noiseX * 0.05f, noiseZ * 0.05f) - 0.5f) * 3f;
                float edgeNoise = Mathf.PerlinNoise(noiseX * 2f, noiseZ * 2f) * 0.5f + 1f;

                for (int y = 0; y < chunkSize + 4; y++)
                {
                    int worldY = CurrentCoordinate.y + y;
                    float normalizedHeight = (float)worldY / WorldHeight;
                    float stepSize = 1f / Steps;
                    float stepProgress = normalizedHeight / stepSize;

                    int currentStep = Mathf.FloorToInt(stepProgress + spatialOffset);
                    currentStep = Mathf.Clamp(currentStep, 0, Steps - 1);
                    float intraStepLerp = stepProgress - currentStep;

                    float baseTaperStart = Mathf.Lerp(0.1f, 1f, (float)currentStep / Steps);
                    float baseTaperEnd = Mathf.Lerp(0.1f, 1f, (float)(currentStep + 1) / Steps);
                    float baseTaper = Mathf.Lerp(baseTaperStart, baseTaperEnd, intraStepLerp);

                    float taperFactor = baseTaper + edgeNoise;
                    float taperedRadius = Radius * taperFactor + 15 * currentStep + wallNoise;
                    float taperedRadiusSquared = taperedRadius * taperedRadius;

                    // Crater carving
                    if (y < chunkSize && distanceSquared <= taperedRadiusSquared)
                    {
                        CurrentChunk[x, y, z] = 0;
                    }

                    // Dirt placement 4 blocks below
                    if (y >= 4 && y - 4 < chunkSize && distanceSquared <= taperedRadiusSquared)
                    {
                        if (CurrentChunk[x, y - 4, z] != 0)
                        {
                            CurrentChunk[x, y - 4, z] = ID;
                        }
                    }
                }
            }
        }
    }
}
