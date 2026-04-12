using UnityEngine;

public class GenTaskEntity : Gen
{
    private static int _id;
    private static int Dirt => _id == 0 ? Block.ConvertID(ID.DirtBlock) : _id;

    private const double DirtTreeChance = 0.02;
    private const double DirtBushChance = 0.02;
    private const double DirtGrassChance = 0.16;
    private const double DirtSheepChance = 0.0005;
    private const double DirtChickenChance = 0.0005;
    private const double DirtSticksChance = 0.01;
    private const double DirtFlintChance = 0.01;

    private const double SurfaceChestChance = 0.0004;
    private const double SurfaceSlabChance = 0.0196;

    public static void Run(Vector3Int currentCoordinate, Chunk currentChunk)
    {
        for (int x = 0; x < World.ChunkSize; x++)
        {
            for (int y = 0; y < World.ChunkSize; y++)
            {
                for (int z = 0; z <  World.ChunkSize; z++)
                {
                    if (
                        y + 1 < World.ChunkSize &&
                        currentChunk[x, y, z] != 0 && 
                        currentChunk[x, y + 1, z] == 0)
                    {
                        Vector3Int position = currentCoordinate + new Vector3Int(x, y + 1, z);
                        double rng = Random.NextDouble();
                        if (currentChunk[x, y, z] == Dirt)
                        {
                            double chance = DirtTreeChance;
                            if (rng <= chance)
                            {
                                ID treeID = Random.NextDouble() <= 0.8 ? ID.PineTree : ID.BirchTree;
                                currentChunk.StaticEntity.Add(Entity.CreateInfo(treeID, position));
                            }
                            else if (rng <= (chance += DirtBushChance))
                            {
                                currentChunk.StaticEntity.Add(Entity.CreateInfo(ID.Bush, position));
                            }
                            else if (rng <= (chance += DirtGrassChance))
                            {
                                currentChunk.StaticEntity.Add(Entity.CreateInfo(ID.Grass, position));
                            }
                            else if (rng <= (chance += DirtSheepChance))
                            {
                                SpawnMobGroup(currentChunk, position, ID.Sheep);
                            }
                            else if (rng <= (chance += DirtChickenChance))
                            {
                                SpawnMobGroup(currentChunk, position, ID.Chicken);
                            }
                            else if (rng <= (chance += DirtSticksChance))
                            {
                                currentChunk.DynamicEntity.Add(Entity.CreateInfo(ID.Sticks, position));
                            }
                            else if (rng <= (chance += DirtFlintChance))
                            {
                                currentChunk.DynamicEntity.Add(Entity.CreateInfo(ID.Flint, position));
                            }
                            else if (rng <= (chance += SurfaceSlabChance))
                            {
                                currentChunk.StaticEntity.Add(Entity.CreateInfo(ID.Slab, position));
                            }
                        }
                        else
                        {
                            double chance = SurfaceChestChance;
                            if (rng <= chance)
                            {
                                currentChunk.StaticEntity.Add(Entity.CreateInfo(ID.Chest, position));
                            } 
                            else if (rng <= (chance += SurfaceSlabChance))
                            {
                                currentChunk.StaticEntity.Add(Entity.CreateInfo(ID.Slab, position));
                            }
                        }
                    }
                }
            }
        }
    }

    private static void SpawnMobGroup(Chunk currentChunk, Vector3Int position, ID mobID)
    {
        int groupCount = Random.Next(1, 4);
        for (int i = 0; i < groupCount; i++)
        {
            currentChunk.DynamicEntity.Add(Entity.CreateInfo(mobID, position));
        }
    }
}