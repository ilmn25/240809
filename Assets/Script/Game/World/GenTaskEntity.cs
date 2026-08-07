using UnityEngine;

public class GenTaskEntity : Gen
{
    private static int _id;
    private static int _idForest;
    private static int Dirt => _id == 0 ? Block.ConvertID(ID.DirtBlock) : _id;
    private static int Sand => _id == 0 ? Block.ConvertID(ID.SandBlock) : _id;
    private static int Forest => _idForest == 0 ? Block.ConvertID(ID.ForestBlock) : _idForest;

    private const double DirtTreeChance = 0.02;
    private const double DirtBushChance = 0.02;
    private const double DirtGrassChance = 0.16;
    private const double SurfaceChestChance = 0.0004;
    private const double SurfaceSlabChance = 0.0196;
    private const double SurfaceSandStructureChance = 0.0196;
    /// <summary>Chance per surface block to spawn a ground item (matches original 1%+1%).</summary>
    private const double GroundItemChance = 0.02;

    // Dense forest generation
    private const double ForestTreeChance = 0.25;
    private const double ForestBushChance = 0.08;
    private const double ForestGrassChance = 0.12;
    private const double ForestDeathcapChance = 0.02;
    private const double GrassOrchidChance = 0.02;
    private static readonly float PathOffset = GetDeterministicOffset("ForestPath");
    private const float PathScale = 0.02f;
    private const float PathWidth = 0.03f;

    /// <summary>Deterministic check for whether a world position lies on a clear path
    /// (a narrow winding band of Perlin noise) where no trees grow.</summary>
    private static bool IsOnPath(int x, int z)
    {
        float noise = Mathf.PerlinNoise(x * PathScale + PathOffset, z * PathScale + PathOffset);
        return noise > 0.5f - PathWidth && noise < 0.5f + PathWidth;
    }

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
                        if (currentChunk[x, y, z] == Forest)
                        {
                            // Forest biome: dense trees, but never on the clear paths.
                            if (IsOnPath(position.x, position.z))
                                continue;

                            double chance = ForestTreeChance;
                            if (roll <= chance)
                            {
                                ID treeID = rng.NextDouble() <= 0.8 ? ID.PineTree : ID.BirchTree;
                                currentChunk.StaticEntity.Add(Entity.CreateInfo(treeID, position));
                            }
                            else if (roll <= (chance += ForestBushChance))
                            {
                                currentChunk.StaticEntity.Add(Entity.CreateInfo(ID.Bush, position));
                            }
                            else if (roll <= (chance += ForestGrassChance))
                            {
                                currentChunk.StaticEntity.Add(Entity.CreateInfo(ID.Grass, position));
                            }
                            else if (roll <= (chance += ForestDeathcapChance))
                            {
                                currentChunk.StaticEntity.Add(Entity.CreateInfo(ID.Deathcap, position));
                            }

                            // Forest floor ground items.
                            if (rng.NextDouble() < GroundItemChance)
                            {
                                ID groundItem = PickGrassItem(rng);
                                if (groundItem != ID.Null)
                                    currentChunk.DynamicEntity.Add(Entity.CreateInfo(groundItem, position));
                            }
                        }
                        else if (currentChunk[x, y, z] == Dirt)
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
                            else if (roll <= (chance += GrassOrchidChance))
                            {
                                currentChunk.StaticEntity.Add(Entity.CreateInfo(ID.Orchids, position));
                            }
                            else if (roll <= (chance += SurfaceSlabChance))
                            {
                                currentChunk.StaticEntity.Add(Entity.CreateInfo(ID.Slab, position));
                            }
 
                            // Grass biome items on dirt surface
                            if (rng.NextDouble() < GroundItemChance)
                            {
                                ID groundItem = PickGrassItem(rng);
                                if (groundItem != ID.Null)
                                    currentChunk.DynamicEntity.Add(Entity.CreateInfo(groundItem, position));
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

                            // Desert biome items on sand surface
                            if (currentChunk[x, y, z] == Sand && rng.NextDouble() < GroundItemChance)
                            {
                                ID groundItem = PickDesertItem(rng);
                                if (groundItem != ID.Null)
                                    currentChunk.DynamicEntity.Add(Entity.CreateInfo(groundItem, position));
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>Desert items — spawn on sand surfaces.</summary>
    private static ID PickDesertItem(System.Random rng)
    {
        double roll = rng.NextDouble();
        double chance = 0;

        if ((chance += 0.30) > roll) return ID.Flint;
        if ((chance += 0.20) > roll) return ID.Shell;
        if ((chance += 0.12) > roll) return ID.Sand;
        return ID.Null;
    }

    private static ID PickGrassItem(System.Random rng)
    {
        double roll = rng.NextDouble();
        double chance = 0;

        if ((chance += 0.45) > roll) return ID.Sticks;
        if ((chance += 0.45) > roll) return ID.Flint;
        if ((chance += 0.10) > roll) return ID.StoneBlock;
        return ID.Null;
    }
}