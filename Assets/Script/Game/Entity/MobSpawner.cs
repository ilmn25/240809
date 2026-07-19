using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DST / Terraria-style mob spawning.
/// Mobs spawn in groups within logic range of each player, up to a per-player cap.
/// Spawn pools are biome-aware and respect day/night cycles.
/// </summary>
public class MobSpawner
{
    private const int SpawnInterval = 200;       // frames between spawn ticks
    private const int MobCapPerPlayer = 15;      // max active mobs near each player
    private const int SpawnAttemptsPerTick = 5;  // retries per tick

    // Day: 0–660 min   Sunset: 660–840   Night: 840–1380   Sunrise: 1380–1440
    private const int NightStart = 840;
    private const int DayStart = 1380;

    private static int _timer;

    private static readonly List<ID> DayMobs = new() { ID.Sheep, ID.Chicken };
    private static readonly List<ID> NightMobs = new() { ID.SnareFlea, ID.Megumin, ID.Slime };
    private static readonly List<ID> DesertNightMobs = new() { ID.SnareFlea };

    public static void Update()
    {
        if (!Helper.IsHost()) return;
        if (Main.BuildMode) return;

        _timer++;
        if (_timer < SpawnInterval) return;
        _timer = 0;

        int totalMobs = EntityDynamicLoad.ActiveEntities.Count;
        int globalCap = Save.Inst.players.Count * MobCapPerPlayer;
        if (totalMobs >= globalCap) return;

        bool isNight = Save.Inst.time >= NightStart || Save.Inst.time < DayStart;

        foreach (var player in Save.Inst.players)
        {
            if (player.Machine == null || player.controllerId == -1) continue;

            // Count active mobs near this player
            int nearby = 0;
            Vector3 pPos = player.Machine.transform.position;
            foreach (var em in EntityDynamicLoad.ActiveEntities)
            {
                if (em == null || em.Info is PlayerInfo) continue;
                if (Vector3.Distance(em.transform.position, pPos) <= Scene.LogicDistance)
                    nearby++;
            }
            if (nearby >= MobCapPerPlayer) continue;

            for (int i = 0; i < SpawnAttemptsPerTick; i++)
                TrySpawnGroup(pPos, isNight);
        }
    }

    private static void TrySpawnGroup(Vector3 playerPos, bool isNight)
    {
        // Pick a random position within logic range (outside render range).
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float dist = Random.Range(Scene.RenderDistance + 2, Scene.LogicDistance - 2);
        Vector3Int spawnPos = Vector3Int.FloorToInt(
            playerPos + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * dist);

        // Snap to surface: scan down from sky to find first solid block.
        if (!FindSurfacePosition(ref spawnPos))
            return;

        // Pick mob pool based on biome and time.
        BiomeType biome = GenHelpBiome.GetBiomeType(spawnPos.x, spawnPos.z);
        List<ID> pool = isNight ? NightMobs : DayMobs;
        if (isNight && biome == BiomeType.Desert)
            pool = DesertNightMobs;

        ID mobID = pool[Random.Range(0, pool.Count)];

        // Spawn a group of 1–3.
        int groupSize = Random.Range(1, 4);
        for (int i = 0; i < groupSize; i++)
        {
            Vector3Int offset = new Vector3Int(Random.Range(-2, 3), 0, Random.Range(-2, 3));
            Entity.Spawn(mobID, spawnPos + offset);
        }
    }

    /// <summary>Scan downward from the given position to find the first
    /// air block directly above a solid block — the surface.</summary>
    private static bool FindSurfacePosition(ref Vector3Int pos)
    {
        int worldBottom = 0;
        int worldTop = World.Inst.Bounds.y;

        // Clamp to world bounds horizontally.
        pos.x = Mathf.Clamp(pos.x, 0, World.Inst.Bounds.x - 1);
        pos.z = Mathf.Clamp(pos.z, 0, World.Inst.Bounds.z - 1);

        // Start at the top of the world and scan down.
        pos.y = worldTop - 1;
        while (pos.y > worldBottom)
        {
            bool currentAir = NavMap.Get(pos);
            pos.y--;
            bool belowSolid = !NavMap.Get(pos);

            if (currentAir && belowSolid)
            {
                // pos.y is now the solid block; surface is one above.
                pos.y++;
                return true;
            }
        }
        return false; // no valid surface found
    }
}