using UnityEngine;

/// <summary>Places one or more raider camps: a central loot chest surrounded by
/// 2-3 dirty tents and a lamp (powered by a nearby generator so it glows).
/// Runs once per world, after chunk generation (like the graveyard).</summary>
public class GenTaskRaiderCamp : GenTaskScatter
{
    private const int CampCount = 2;        // how many separate camps to place
    private const int MinTents = 2;
    private const int MaxTents = 3;
    private const int ClusterRadius = 4;    // how far structures scatter from the chest

    /// <summary>Places the raider camps, if terrain permits.</summary>
    public override void RunWorld(World world)
    {
        System.Random rng = Gen.CreateWorldRandom("RaiderCamp");

        for (int i = 0; i < CampCount; i++)
        {
            Vector3Int column = PickGrassCenter(world, rng);
            if (column.x < 0) continue;
            int surfaceY = FindSurfaceY(world, column.x, column.z);
            if (surfaceY < 0) continue;
            PlaceCamp(world, new Vector3Int(column.x, surfaceY, column.z), rng);
        }
    }

    /// <summary>Places a camp cluster around the given surface center.</summary>
    private static void PlaceCamp(World world, Vector3Int center, System.Random rng)
    {
        // Central loot chest (with the standard chest loot table).
        ContainerInfo chest = (ContainerInfo)Entity.CreateInfo(ID.Chest, center);
        Loot.Gettable(ID.Chest).AddToContainer(chest.Storage);
        PlaceInfo(world, center, chest);

        int tentCount = rng.Next(MinTents, MaxTents + 1);
        for (int i = 0; i < tentCount; i++)
        {
            Vector3Int spot = ScatterAround(world, center, rng, ClusterRadius);
            if (spot.x < 0) continue;
            PlaceEntity(world, spot, ID.DirtyTent);
        }

        // A lamp so it glows at the camp.
        Vector3Int lampSpot = ScatterAround(world, center, rng, ClusterRadius);
        if (lampSpot.x >= 0) PlaceEntity(world, lampSpot, ID.Lamp);
    }
}