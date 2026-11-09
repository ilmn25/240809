using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Random = UnityEngine.Random;
using Pool = System.Collections.Generic.List<(ID id, float weight)>;

/// <summary>How dangerous a world is. Picked when a new game starts. Normal is 0 so
/// saves written before this setting existed keep spawning as before.</summary>
public enum MobDifficulty { Normal = 0, Peaceful = 1, Overwhelming = 2 }

/// <summary>Spawns wildlife and hostiles around players. One <see cref="Rules"/> profile
/// describes a spawn pulse (how often, how many attempts per player, how many mobs may
/// live nearby, whether hostiles exist at all), so tuning a difficulty touches one place
/// instead of a pile of scattered constants.</summary>
public static class MobSpawner
{
    private const float DayRoamerChance = 0.5f;
    private const float PassiveRarity = 0.1f;
    private const int PassiveCap = 24;
    private const float BoundNPCChance = 0.002f;
    private const int CrowFlockMin = 2;
    private const int CrowFlockMax = 3;
    private const int CrowCap = 8;           // crows kept alive near a player (ambience, not a swarm)
    private const float CrowSpawnNear = 8f;  // closest a flock lands to the player

    private enum SpawnPhase { Day, Night, Rapture }

    private readonly struct Rules
    {
        public readonly float Interval;
        public readonly int Attempts;
        public readonly int CapPerPlayer;
        public readonly bool Hostiles;

        public Rules(float interval, int attempts, int capPerPlayer, bool hostiles)
        {
            Interval = interval;
            Attempts = attempts;
            CapPerPlayer = capPerPlayer;
            Hostiles = hostiles;
        }
    }

    private static readonly Pool DayRoamers = new()
    {
        (ID.Slime, 2.1f), (ID.Crow, 0.25f), (ID.Pigeon, 0.2f), (ID.Gnome, 0.1f),
    };

    // Crow weights are per flock (each flock is 2-3 birds), not per bird.
    private static readonly Dictionary<BiomeType, Pool> DayByBiome = new()
    {
        [BiomeType.Grass] = new Pool { (ID.Sheep, 1f), (ID.Crow, 0.25f) },
        [BiomeType.Forest] = new Pool { (ID.Hen, 1f), (ID.Rooster, 1f), (ID.Chick, 1f), (ID.Crow, 0.5f) },
    };

    private static readonly Pool DesertNightMobs = Uniform(ID.SnareFlea);
    private static readonly Pool RaptureMobs = Uniform(ID.Lich);
    private static readonly Pool DungeonMobs = Uniform(ID.Sawblade, ID.Ballista, ID.Turret);
    /// <summary>Deeper dungeon floors: the traps stay, and the restless dead join them.</summary>
    private static readonly Pool DeepDungeonMobs = Uniform(ID.Sawblade, ID.Ballista, ID.Turret,
        ID.Skeleton, ID.Rat, ID.Spider, ID.Vampire);
    private static readonly Pool NightMobs = Uniform(ID.SnareFlea, ID.Raider, ID.Scavenger,
        ID.ScavengerArcher, ID.Congregant,
        ID.Acolyte, ID.Heretic, ID.Cultist, ID.Bear, ID.Watchdog, ID.TreeMimic, ID.Mannequin,
        ID.Vampire, ID.Sawblade, ID.Ballista, ID.Turret);

    private static float _nextPulse;

    public static void Update()
    {
        if (!Helper.IsHost() || Main.CreativeMode || Save.Inst == null) return;

        Rules rules = GetRules();
        if (Time.time < _nextPulse) return;
        _nextPulse = Time.time + rules.Interval;

        bool dungeon = Save.Inst.current == GenType.Dungeon;
        if (!dungeon && CountMobs() >= Save.Inst.players.Count * rules.CapPerPlayer) return;

        SpawnPhase phase = GetPhase();
        foreach (PlayerInfo player in Save.Inst.players)
        {
            if (player.Machine == null || player.controllerId == -1) continue;

            Vector3 playerPos = player.Machine.transform.position;
            if (CountMobs(near: playerPos, radius: Scene.LogicDistance) >= rules.CapPerPlayer) continue;

            for (int i = 0; i < rules.Attempts; i++)
            {
                if (dungeon) TrySpawnDungeonMob(playerPos, rules.Hostiles);
                else TrySpawnAround(playerPos, rules, phase);
            }
        }
    }

    /// <summary>Difficulty sets the baseline pressure; swarms (Rapture, full moon) and
    /// dungeons deviate from it.</summary>
    private static Rules GetRules()
    {
        Rules rules = Save.Inst.difficulty switch
        {
            MobDifficulty.Peaceful => new Rules(4f, 2, 6, false),
            MobDifficulty.Overwhelming => new Rules(1.2f, 10, 20, true),
            _ => new Rules(2.4f, 5, 10, true),
        };

        if (Save.Inst.current == GenType.Dungeon)
        {
            // Deeper floors press harder: more spawn attempts per pulse.
            int deeper = Mathf.Min(Save.Inst.DungeonDepth - 1, 4);
            return new Rules(rules.Interval, rules.Attempts + 1 + deeper, 3, rules.Hostiles);
        }
        if (IsSwarm())
            return new Rules(rules.Interval * 0.5f, rules.Attempts + 3, rules.CapPerPlayer * 3, rules.Hostiles);
        return rules;
    }

    private static SpawnPhase GetPhase() => Save.Inst.weather switch
    {
        EnvironmentType.Rapture => SpawnPhase.Rapture,
        EnvironmentType.NightRainy or EnvironmentType.NightBright => SpawnPhase.Night,
        _ => SpawnPhase.Day,
    };

    private static bool IsSwarm() =>
        Save.Inst.weather is EnvironmentType.Rapture or EnvironmentType.NightBright;

    private static void TrySpawnDungeonMob(Vector3 playerPos, bool hostiles)
    {
        if (!hostiles) return;

        Vector3Int ground = new(
            Mathf.RoundToInt(playerPos.x + Random.Range(-Scene.RenderDistance, Scene.RenderDistance)),
            0,
            Mathf.RoundToInt(playerPos.z + Random.Range(-Scene.RenderDistance, Scene.RenderDistance)));

        if (!World.IsInWorldBounds(ground) || NavMap.IsAir(ground) || !NavMap.IsAir(ground + Vector3Int.up))
            return;

        Entity.Spawn(PickWeighted(Save.Inst.DungeonDepth > 2 ? DeepDungeonMobs : DungeonMobs),
                     ground + Vector3Int.up);
    }

    private static void TrySpawnAround(Vector3 playerPos, Rules rules, SpawnPhase phase)
    {
        // A rare wanderer may be rescued during the day instead of a wild spawn.
        if (phase == SpawnPhase.Day && rules.Hostiles && Random.value < BoundNPCChance &&
            TryFindSpawnPoint(playerPos, Scene.RenderDistance + 2, Scene.LogicDistance - 2, out Vector3Int npcPos))
        {
            Entity.Spawn(ID.BoundNPC, npcPos);
            return;
        }

        ID? picked = PickMob(playerPos, phase, rules.Hostiles);
        if (picked == null) return;
        ID id = picked.Value;

        if (id == ID.Crow)
        {
            // Flocks have their own cap so a sky full of birds never crowds out the
            // other daytime spawns sharing the global mob cap.
            if (CountMobs(m => m.id == ID.Crow, near: playerPos, radius: Scene.LogicDistance) >= CrowCap) return;

            // Crows land a bit closer than other spawns so flocks arrive in view.
            for (int i = Random.Range(CrowFlockMin, CrowFlockMax + 1); i > 0; i--)
                if (TryFindSpawnPoint(playerPos, CrowSpawnNear, Scene.RenderDistance - 2f, out Vector3Int crowPos))
                    Entity.Spawn(ID.Crow, crowPos);
            return;
        }

        if (!TryFindSpawnPoint(playerPos, Scene.RenderDistance + 2, Scene.LogicDistance - 2, out Vector3Int pos)) return;
        for (int i = GroupSize(id); i > 0; i--)
            Entity.Spawn(id, pos);
    }

    /// <summary>Rolls one mob for this phase, or null when nothing may spawn (no pool for
    /// the biome, passive mob held back by rarity or cap, hostiles in a peaceful world).</summary>
    private static ID? PickMob(Vector3 playerPos, SpawnPhase phase, bool hostiles)
    {
        BiomeType biome = GenHelpBiome.GetBiomeType(
            Mathf.RoundToInt(playerPos.x), Mathf.RoundToInt(playerPos.z));

        Pool pool = phase switch
        {
            SpawnPhase.Rapture => RaptureMobs,
            SpawnPhase.Night => biome == BiomeType.Desert ? DesertNightMobs : NightMobs,
            _ => Random.value < DayRoamerChance
                ? DayRoamers
                : DayByBiome.TryGetValue(biome, out Pool byBiome) ? byBiome : null,
        };

        if (pool == null || pool.Count == 0) return null;
        if (!hostiles) pool = pool.Where(e => IsHarmless(e.id)).ToList();
        if (pool.Count == 0) return null;

        ID id = PickWeighted(pool);
        if (IsPassiveMob(id) &&
            (Random.value < PassiveRarity || CountMobs(m => IsPassiveMob(m.id)) >= PassiveCap))
            return null;
        return id;
    }

    private static bool TryFindSpawnPoint(Vector3 origin, float minDistance, float maxDistance, out Vector3Int position)
    {
        float angle = Random.Range(0f, Mathf.PI * 2f);
        position = Vector3Int.FloorToInt(
            origin + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * Random.Range(minDistance, maxDistance));
        return NavMap.TryFindSurface(ref position);
    }

    private static int GroupSize(ID id) => id switch
    {
        ID.Sheep => Random.Range(1, 3),
        _ => 1,
    };

    public static int CountMobs(Func<MobInfo, bool> where = null, Vector3 near = default, float radius = 0f) =>
        EntityDynamicLoad.ActiveEntities.Count(em =>
            em?.Info is MobInfo mob && mob is not PlayerInfo &&
            (radius <= 0f || Vector3.Distance(em.transform.position, near) <= radius) &&
            (where == null || where(mob)));

    // Livestock: harmless, so they spawn in rarer, capped groups.
    private static bool IsPassiveMob(ID id) => id is ID.Sheep or ID.Hen or ID.Rooster or ID.Chick;

    // Harmless wildlife that may still show up when mobs are set to peaceful.
    private static bool IsHarmless(ID id) => IsPassiveMob(id) || id is ID.Crow or ID.Pigeon;

    private static Pool Uniform(params ID[] ids) => ids.Select(id => (id, 1f)).ToList();

    private static ID PickWeighted(Pool pool)
    {
        float roll = Random.value * pool.Sum(e => e.weight);
        foreach (var (id, weight) in pool)
            if ((roll -= weight) <= 0f) return id;
        return pool[^1].id;
    }
}