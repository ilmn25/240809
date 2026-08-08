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
    // During Rapture / full moon, spawn more enemies.
    private const int EventMobCapPerPlayer = 30;
    private const int EventSpawnAttemptsPerTick = 10;

    private static int _timer;

    private static readonly List<ID> DayMobs = new() { ID.Sheep, ID.Hen, ID.Rooster, ID.Chick };
    private static readonly List<ID> NightMobs = new() { ID.SnareFlea, ID.Megumin, ID.Slime };
    private static readonly List<ID> DesertNightMobs = new() { ID.SnareFlea };

    /// <summary>True during Rapture or a full-moon (bright) night — spawns ramp up.</summary>
    private static bool IsEventActive =>
        Save.Inst.weather == EnvironmentType.Rapture ||
        Save.Inst.weather == EnvironmentType.NightBright;

    public static void Update()
    {
        if (!Helper.IsHost()) return;
        if (Main.CreativeMode) return;

        _timer++;
        if (_timer < SpawnInterval) return;
        _timer = 0;

        bool eventActive = IsEventActive;
        int capPerPlayer = eventActive ? EventMobCapPerPlayer : MobCapPerPlayer;
        int attemptsPerTick = eventActive ? EventSpawnAttemptsPerTick : SpawnAttemptsPerTick;

        int totalMobs = EntityDynamicLoad.ActiveEntities.Count;
        int globalCap = Save.Inst.players.Count * capPerPlayer;
        if (totalMobs >= globalCap) return;

        bool isNight = Save.Inst.weather == EnvironmentType.NightRainy || Save.Inst.weather == EnvironmentType.NightBright;

        foreach (var player in Save.Inst.players)
        {
            if (player.Machine == null || player.controllerId == -1) continue;

            int nearby = 0;
            Vector3 pPos = player.Machine.transform.position;
            foreach (var em in EntityDynamicLoad.ActiveEntities)
            {
                if (em == null || em.Info is PlayerInfo) continue;
                if (Vector3.Distance(em.transform.position, pPos) <= Scene.LogicDistance)
                    nearby++;
            }
            if (nearby >= capPerPlayer) continue;

            for (int i = 0; i < attemptsPerTick; i++)
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

        if (!FindSurfacePosition(ref spawnPos))
            return;

        BiomeType biome = GenHelpBiome.GetBiomeType(spawnPos.x, spawnPos.z);
        List<ID> pool = isNight ? NightMobs : DayMobs;
        if (isNight && biome == BiomeType.Desert)
            pool = DesertNightMobs;

        ID mobID = pool[Random.Range(0, pool.Count)];

        // Sheep graze in large pure flocks.
        if (mobID == ID.Sheep)
        {
            int herd = Random.Range(5, 7);
            for (int i = 0; i < herd; i++)
                Entity.Spawn(ID.Sheep, spawnPos);
            return;
        }

        // Poultry spawn as a mixed farmyard flock: hens, chicks, and usually a rooster.
        for (int i = 0, n = Random.Range(1, 3); i < n; i++) // 1–2 hens
            Entity.Spawn(ID.Hen, spawnPos);
        for (int i = 0, n = Random.Range(2, 5); i < n; i++) // 2–4 chicks
            Entity.Spawn(ID.Chick, spawnPos);
        if (Random.value < 0.6f)
            Entity.Spawn(ID.Rooster, spawnPos);
    }

    /// <summary>Scan downward from the given position to find the first
    /// air block directly above a solid block — the surface.</summary>
    private static bool FindSurfacePosition(ref Vector3Int pos)
    {
        int worldBottom = 0;
        int worldTop = World.Inst.Bounds.y;

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