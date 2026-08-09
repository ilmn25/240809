using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Terraria-style mob spawning.
/// Mobs spawn in groups within logic range of each player, up to a per-player cap.
/// Spawn pools are biome-aware and respect day/night cycles.
/// </summary>
public class MobSpawner
{
    private const int SpawnInterval = 200;       // frames between spawn ticks
    private const int MobCapPerPlayer = 15;      // max active mobs near each player
    private const int SpawnAttemptsPerTick = 5;  // retries per tick
    /// <summary>Chance a passive farm-animal group (sheep/poultry) actually
    /// spawns — 0.5 makes them 2x rarer.</summary>
    private const float PassiveMobRarity = 0.5f;
    // During Rapture / full moon, spawn more enemies.
    private const int EventMobCapPerPlayer = 30;
    private const int EventSpawnAttemptsPerTick = 10;

    private static int _timer;

    private static readonly List<ID> GrassMobs = new() { ID.Sheep };              // sheep graze only on grassland
    private static readonly List<ID> ForestMobs = new() { ID.Hen, ID.Rooster, ID.Chick }; // poultry only in forest
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
        List<ID> pool;
        if (isNight)
            pool = biome == BiomeType.Desert ? DesertNightMobs : NightMobs;
        else
            pool = biome switch
            {
                BiomeType.Grass => GrassMobs,   // sheep graze only on grassland
                BiomeType.Forest => ForestMobs, // poultry live only in forest
                _ => null,                      // no passive mobs in other biomes
            };

        if (pool == null || pool.Count == 0) return;

        ID mobID = pool[Random.Range(0, pool.Count)];

        // Passive farm animals (sheep & poultry) are 2x rarer — only 50% of
        // the time does a day group actually spawn.
        if (!isNight && Random.value < PassiveMobRarity)
            return;

        // Sheep graze in small flocks (half of the previous 5–6).
        if (mobID == ID.Sheep)
        {
            int herd = Random.Range(2, 4);
            for (int i = 0; i < herd; i++)
                Entity.Spawn(ID.Sheep, spawnPos);
            return;
        }

        // Poultry spawn as a small farmyard flock: a hen, a couple of chicks,
        // and sometimes a rooster (amounts halved).
        for (int i = 0, n = Random.Range(1, 2); i < n; i++) // 1 hen
            Entity.Spawn(ID.Hen, spawnPos);
        for (int i = 0, n = Random.Range(1, 3); i < n; i++) // 1–2 chicks
            Entity.Spawn(ID.Chick, spawnPos);
        if (Random.value < 0.3f)
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