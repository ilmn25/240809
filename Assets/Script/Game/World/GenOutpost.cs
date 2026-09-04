using UnityEngine;

public class GenOutpost : GenTaskScatter
{
    private static readonly Chunk Outpost = SetPiece.LoadSetPieceFile("Outpost");

    public override void RunWorld(World world)
    {
        if (Outpost == null) return;
        System.Random rng = Gen.CreateWorldRandom("Outpost");

        Vector3Int origin = PickFootprintOrigin(world, rng, Outpost.size);
        if (origin.x < 0) return;

        SetPiece.Paste(world, origin, Outpost);

        Vector3Int tentSpot = FindTentSpot(world, origin.x, origin.z, rng);
        if (tentSpot.x >= 0)
            PlaceEntity(world, tentSpot, ID.MercenaryTent);
    }

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
