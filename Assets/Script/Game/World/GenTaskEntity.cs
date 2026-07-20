using UnityEngine;

public class GenTaskEntity : Gen
{
    private static int _id;
    private static int Dirt => _id == 0 ? Block.ConvertID(ID.DirtBlock) : _id;
    private static int Sand => _id == 0 ? Block.ConvertID(ID.SandBlock) : _id;

    private const double DirtTreeChance = 0.02;
    private const double DirtBushChance = 0.02;
    private const double DirtGrassChance = 0.16;
    private const double SurfaceChestChance = 0.0004;
    private const double SurfaceSlabChance = 0.0196;
    private const double SurfaceSandStructureChance = 0.0196;

    public static void Run(Vector3Int currentCoordinate, Chunk currentChunk)
    {
        System.Random rng = CreateChunkRandom("Entity", currentCoordinate);

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
                        double roll = rng.NextDouble();
                        if (currentChunk[x, y, z] == Dirt)
                        {
                            double chance = DirtTreeChance;
                            if (roll <= chance)
                            {
                                ID treeID = rng.NextDouble() <= 0.8 ? ID.PineTree : ID.BirchTree;
                                currentChunk.StaticEntity.Add(Entity.CreateInfo(treeID, position));
                            }
                            else if (roll <= (chance += DirtBushChance))
                            {
                                currentChunk.StaticEntity.Add(Entity.CreateInfo(ID.Bush, position));
                            }
                            else if (roll <= (chance += DirtGrassChance))
                            {
                                currentChunk.StaticEntity.Add(Entity.CreateInfo(ID.Grass, position));
                            }
                            else if (roll <= (chance += SurfaceSlabChance))
                            {
                                currentChunk.StaticEntity.Add(Entity.CreateInfo(ID.Slab, position));
                            }
                        }
                        else
                        {
                            double chance = SurfaceChestChance;
                            bool isDesert = GenHelpBiome.GetBiomeType(position.x, position.z) == BiomeType.Desert;
                            if (roll <= chance)
                            {
                                currentChunk.StaticEntity.Add(Entity.CreateInfo(ID.Chest, position));
                            } 
                            else if (isDesert && roll <= (chance += SurfaceSandStructureChance))
                            {
                                ID spawnID = rng.NextDouble() <= 0.5 ? ID.SandSlab : ID.SandDebris;
                                currentChunk.StaticEntity.Add(Entity.CreateInfo(spawnID, position));
                            }
                            else if (roll <= (chance += SurfaceSlabChance))
                            {
                                currentChunk.StaticEntity.Add(Entity.CreateInfo(ID.Slab, position));
                            }
                        }

                        // Surface ground pickups — persistent worldgen items
                        if (roll >= 0.9)
                        {
                            ID groundItem = PickGroundItem(rng, position);
                            if (groundItem != ID.Null)
                                currentChunk.StaticEntity.Add(Entity.CreateInfo(groundItem, position));
                        }
                    }
                }
            }
        }
    }

    private static ID PickGroundItem(System.Random rng, Vector3Int position)
    {
        BiomeType biome = GenHelpBiome.GetBiomeType(position.x, position.z);
        return biome == BiomeType.Desert
            ? PickDesertItem(rng)
            : PickGrassItem(rng);
    }

    private static ID PickDesertItem(System.Random rng)
    {
        double roll = rng.NextDouble();
        double chance = 0;

        if ((chance += 0.25) > roll) return ID.Flint;
        if ((chance += 0.20) > roll) return ID.Gravel;
        if ((chance += 0.15) > roll) return ID.Shell;
        if ((chance += 0.12) > roll) return ID.Sand;
        if ((chance += 0.08) > roll) return ID.Sticks;
        if ((chance += 0.03) > roll) return ID.MetalChunks;
        if ((chance += 0.02) > roll) return ID.CopperChunks;
        return ID.Null;
    }

    private static ID PickGrassItem(System.Random rng)
    {
        double roll = rng.NextDouble();
        double chance = 0;

        if ((chance += 0.22) > roll) return ID.Flint;
        if ((chance += 0.20) > roll) return ID.Sticks;
        if ((chance += 0.12) > roll) return ID.Gravel;
        if ((chance += 0.10) > roll) return ID.Shell;
        if ((chance += 0.10) > roll) return ID.Acorn;
        if ((chance += 0.08) > roll) return ID.Mud;
        if ((chance += 0.03) > roll) return ID.MetalChunks;
        if ((chance += 0.02) > roll) return ID.CopperChunks;
        return ID.Null;
    }
}