using UnityEngine;

public class GenTaskRaiderCamp : GenTaskScatter
{
    private const int CampCount = 2;
    private const int MinTents = 2;
    private const int MaxTents = 3;
    private const int ClusterRadius = 4;

    public override void RunWorld(World world)
    {
        System.Random rng = Gen.CreateWorldRandom("RaiderCamp");
        int campSize = ClusterRadius * 2 + 1;

        for (int i = 0; i < CampCount; i++)
        {
            Vector3Int column = FindCampCenter(world, rng, campSize);
            if (column.x < 0) continue;
            PlaceCamp(world, column, rng);
        }
    }

    private static Vector3Int FindCampCenter(World world, System.Random rng, int campSize)
    {
        Vector3Int column = PickGrassCenter(world, rng, 40,
            c => FindFootprintSurface(world, c.x - ClusterRadius, c.z - ClusterRadius, campSize, maxSpread: 3) >= 0);
        if (column.x < 0) return column;
        return new Vector3Int(column.x, FindSurfaceY(world, column.x, column.z), column.z);
    }

    private static void PlaceCamp(World world, Vector3Int center, System.Random rng)
    {
        PlaceEntity(world, center, ID.RaiderCampChest);

        int tentCount = rng.Next(MinTents, MaxTents + 1);
        for (int i = 0; i < tentCount; i++)
        {
            Vector3Int spot = ScatterAround(world, center, rng, ClusterRadius);
            if (spot.x < 0) continue;
            PlaceEntity(world, spot, ID.DirtyTent);
        }

        Vector3Int lampSpot = ScatterAround(world, center, rng, ClusterRadius);
        if (lampSpot.x >= 0) PlaceEntity(world, lampSpot, ID.Lamp);
    }
}