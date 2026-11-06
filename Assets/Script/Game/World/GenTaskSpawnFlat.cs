using UnityEngine;

/// <summary>Levels the ground around the world spawn into a flat pad, easing back into the
/// natural terrain at the rim so the pad has no cliff edge. Runs once per world, before any
/// spawn-area structures or entities are placed.</summary>
public class GenTaskSpawnFlat : GenTaskScatter
{
    private const int FlatRadius = 12;   // completely level inside this
    private const int BlendRadius = 24;  // fully natural terrain from here out

    private static int _grass;
    private static int Grass => _grass == 0 ? (_grass = Block.ConvertID(ID.GrassBlock)) : _grass;

    public override void RunWorld(World world)
    {
        Vector3Int spawn = world.SpawnPoint;
        int flatY = FindSurfaceY(world, spawn.x, spawn.z);
        if (flatY < 0) return;

        ForEachColumnAround(spawn, BlendRadius, (x, z) =>
        {
            int surfaceY = FindSurfaceY(world, x, z);
            if (surfaceY < 0) return;

            // Ease from the flat height to this column's natural height across the rim.
            float distance = new Vector2(x - spawn.x, z - spawn.z).magnitude;
            float rim = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(FlatRadius, BlendRadius, distance));
            int top = Mathf.RoundToInt(Mathf.Lerp(flatY, surfaceY, rim)) - 1;
            if (top <= 0) return;

            for (int y = surfaceY; y <= top; y++) GenBlocks.SetBlock(world, x, y, z, Grass);
            for (int y = top + 1; y < surfaceY; y++) GenBlocks.SetBlock(world, x, y, z, 0);
            GenBlocks.SetBlock(world, x, top, z, Grass);
        });
    }
}
