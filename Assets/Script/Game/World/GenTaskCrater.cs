using UnityEngine;

public class GenTaskCrater : Gen
{
    private static readonly int CenterX = World.Inst.Bounds.x/2;
    private static readonly int CenterZ = World.Inst.Bounds.z/2;
    private static readonly int Radius = 24;
    private const float Scale = 0.1f;
    private static readonly int Steps = 16;
    private static readonly float Offset = GetOffset();
    private static int _id;
    private static int Dirt => _id == 0 ? Block.ConvertID(ID.DirtBlock) : _id; 

    public static void Run(Vector3Int currentCoordinate, Chunk currentChunk)
    {
        int chunkSize = World.ChunkSize;

        for (int x = 0; x < chunkSize; x++)
        {
            int worldX = currentCoordinate.x + x;
            int distanceX = Mathf.Abs(CenterX - worldX);
            float noiseX = worldX * Scale + Offset;

            for (int z = 0; z < chunkSize; z++)
            {
                int worldZ = currentCoordinate.z + z;
                int distanceZ = Mathf.Abs(CenterZ - worldZ);
                float noiseZ = worldZ * Scale + Offset;

                float distanceSquared = distanceX * distanceX + distanceZ * distanceZ;
                float wallNoise = (Mathf.PerlinNoise(noiseX, noiseZ) * 2f - 1f) * 5f;
                float spatialOffset = (Mathf.PerlinNoise(noiseX * 0.05f, noiseZ * 0.05f) - 0.5f) * 3f;
                float edgeNoise = Mathf.PerlinNoise(noiseX * 2f, noiseZ * 2f) * 0.5f + 1f;

                for (int y = 0; y < chunkSize + 4; y++)
                {
                    int worldY = currentCoordinate.y + y;
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
                        currentChunk[x, y, z] = 0;
                    }

                    // Dirt placement 4 blocks below
                    if (y >= 4 && y - 4 < chunkSize && distanceSquared <= taperedRadiusSquared)
                    {
                        if (currentChunk[x, y - 4, z] != 0)
                        {
                            currentChunk[x, y - 4, z] = Dirt;
                        }
                    }
                }
            }
        }
    }
}
