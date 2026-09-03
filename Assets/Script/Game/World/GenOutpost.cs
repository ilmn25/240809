using UnityEngine;

/// <summary>Places a fortified outpost watchtower (Set/Outpost.json) in the grass
/// biome, with a hireable mercenary camping in a tent beside it. Runs once per
/// world, after chunk generation (like the raider camp).</summary>
public class GenOutpost : GenTaskScatter
{
    private static readonly Chunk Outpost = SetPiece.LoadSetPieceFile("Outpost");

    public override void RunWorld(World world)
    {
        if (Outpost == null) return;
        System.Random rng = Gen.CreateWorldRandom("Outpost");
        Vector3Int column = PickGrassCenter(world, rng);
        if (column.x < 0) return;
        int surfaceY = FindSurfaceY(world, column.x, column.z);
        if (surfaceY < 0) return;
        SetPiece.Paste(world, new Vector3Int(column.x, surfaceY, column.z), Outpost);

        // A hireable mercenary camps in a tent beside the tower.
        Vector3Int tentSpot = FindTentSpot(world, column.x, column.z, rng);
        if (tentSpot.x >= 0)
            PlaceEntity(world, tentSpot, ID.MercenaryTent);
    }

    // A clear grass cell a short distance out from the tower (clear of its 7x7
    // footprint), or (-1,0,0) if none is found.
    private static Vector3Int FindTentSpot(World world, int centerX, int centerZ, System.Random rng)
    {
        int[] distances = { 8, 6, 10 };
        for (int i = 0; i < 12; i++)
        {
            int distance = distances[i % distances.Length];
            bool horizontal = (i / 2) % 2 == 0;
            int sign = i % 2 == 0 ? 1 : -1;
            int x = horizontal ? centerX + distance * sign : centerX + rng.Next(-2, 3);
            int z = horizontal ? centerZ + rng.Next(-2, 3) : centerZ + distance * sign;
            int y = FindSurfaceY(world, x, z);
            if (y < 0) continue;
            return new Vector3Int(x, y, z);
        }
        return new Vector3Int(-1, 0, 0);
    }
}
