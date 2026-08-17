using UnityEngine;

/// <summary>Scatters a few ponds across the world and rings each with bushes, so
/// bushes only appear right next to water. Runs once per world, after chunk gen.</summary>
public class GenTaskPond : GenTaskScatter
{
    private const int MinPonds = 4;
    private const int MaxPonds = 8;
    private const int Attempts = 30;

    public static void Run(World world)
    {
        System.Random rng = new System.Random((int)GetDeterministicOffset("Ponds"));
        int count = rng.Next(MinPonds, MaxPonds + 1);
        for (int i = 0; i < count; i++)
            TryPlacePond(world, rng);
    }

    private static void TryPlacePond(World world, System.Random rng)
    {
        for (int attempt = 0; attempt < Attempts; attempt++)
        {
            int x = rng.Next(2, world.Bounds.x - 2);
            int z = rng.Next(2, world.Bounds.z - 2);
            int surfaceY = FindSurfaceY(world, x, z);
            if (surfaceY < 0) continue;

            PlaceEntity(world, new Vector3Int(x, surfaceY, z), ID.Pond);

            // Ring the pond with bushes — they only grow by water.
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dz = -1; dz <= 1; dz++)
                {
                    if (dx == 0 && dz == 0) continue;
                    if (rng.NextDouble() > 0.5) continue;
                    int bx = x + dx;
                    int bz = z + dz;
                    if (bx < 1 || bz < 1 || bx >= world.Bounds.x - 1 || bz >= world.Bounds.z - 1) continue;
                    int by = FindSurfaceY(world, bx, bz);
                    if (by != surfaceY) continue;
                    PlaceEntity(world, new Vector3Int(bx, by, bz), ID.Bush);
                }
            }
            return;
        }
    }
}
