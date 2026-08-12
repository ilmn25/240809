using UnityEngine;

public class GenTaskEntity : Gen
{
    private static int _id;
    private static int _idForest;
    private static int Dirt => _id == 0 ? Block.ConvertID(ID.GrassBlock) : _id;
    private static int Sand => _id == 0 ? Block.ConvertID(ID.SandBlock) : _id;
    private static int Forest => _idForest == 0 ? Block.ConvertID(ID.ForestBlock) : _idForest;

    private const double DirtTreeChance = 0.0025;
    private const double DirtBushChance = 0.00125;
    private const double DirtGrassChance = 0.08;
    private const double SurfaceChestChance = 0.0002;
    private const double SurfaceSkeletonChance = 0.0001;
    private const double SurfaceSlabChance = 0.0098;
    private const double SurfaceSandStructureChance = 0.0098;
    /// <summary>Chance per surface block to spawn a ground item.</summary>
    private const double GroundItemChance = 0.016;

    // Dense forest generation
    private const double ForestTreeChance = 0.0625;
    private const double ForestBushChance = 0.005;
    private const double ForestGrassChance = 0.06;
    private const double ForestDeathcapChance = 0.00125;
    private const double ForestSpiderNestChance = 0.0008;
    private const double GrassOrchidChance = 0.005;
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
                        if (rng.NextDouble() < SurfaceSkeletonChance)
                        {
                            currentChunk.StaticEntity.Add(Entity.CreateInfo(ID.Skeleton, position));
                            SpawnSkeletonLoot(currentCoordinate, currentChunk, position, rng);
                            continue; // one structure per cell — skip the biome spawn
                        }
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
                            else if (roll <= (chance += ForestSpiderNestChance))
                            {
                                currentChunk.StaticEntity.Add(Entity.CreateInfo(ID.SpiderNest, position));
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
                                ContainerInfo chest = (ContainerInfo)Entity.CreateInfo(ID.Chest, position);
                                Loot.Gettable(ID.Chest).AddToContainer(chest.Storage);
                                currentChunk.StaticEntity.Add(chest);
                            } 
                            else if (isDesert && roll <= (chance += SurfaceSandStructureChance))
                            {
                                ID spawnID = rng.NextDouble() <= 0.5 ? ID.SandSlab : ID.SandDebris;
                                currentChunk.StaticEntity.Add(Entity.CreateInfo(spawnID, position));
                            }
                            else if (!isDesert && roll <= (chance += SurfaceSlabChance))
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

        if ((chance += 0.02) > roll) return ID.Flint;
        if ((chance += 0.20) > roll) return ID.Shell;
        if ((chance += 0.12) > roll) return ID.Sand;
        return ID.Null;
    }

    private static ID PickGrassItem(System.Random rng)
    {
        double roll = rng.NextDouble();
        double chance = 0;

        if ((chance += 0.45) > roll) return ID.Sticks;
        if ((chance += 0.45) > roll) return ID.Mud;
        if ((chance += 0.45) > roll) return ID.Flint;
        if ((chance += 0.10) > roll) return ID.Gravel;
        return ID.Null;
    }

    // Scatters low-tier starter loot (crude tools, flint) around a skeleton.
    private static void SpawnSkeletonLoot(Vector3Int currentCoordinate, Chunk currentChunk, Vector3Int position, System.Random rng)
    {
        int count = rng.Next(1, 3);
        for (int i = 0; i < count; i++)
        {
            int lx = position.x + rng.Next(-1, 2);
            int lz = position.z + rng.Next(-1, 2);
            int localX = lx - currentCoordinate.x;
            int localZ = lz - currentCoordinate.z;
            if (localX < 0 || localX >= World.ChunkSize || localZ < 0 || localZ >= World.ChunkSize) continue;
            ID item = PickSkeletonLoot(rng);
            if (item != ID.Null)
                currentChunk.DynamicEntity.Add(Entity.CreateInfo(item, new Vector3Int(lx, position.y, lz)));
        }
    }

    private static ID PickSkeletonLoot(System.Random rng)
    {
        double roll = rng.NextDouble();
        double chance = 0;
        if ((chance += 0.25) > roll) return ID.CrudeHatchet;
        if ((chance += 0.25) > roll) return ID.CrudePickaxe;
        if ((chance += 0.2) > roll) return ID.CrudeMallet;
        if ((chance += 0.2) > roll) return ID.Flint;
        return ID.Sticks;
    }
}