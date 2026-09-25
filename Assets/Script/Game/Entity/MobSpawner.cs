using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Random = UnityEngine.Random;
using Pool = System.Collections.Generic.List<(ID id, float weight)>;

public class MobSpawner
{
    private const int SpawnInterval = 240;
    private const int SpawnAttemptsPerTick = 5;
    private const int MobCapPerPlayer = 18;
    private const int EventSpawnAttemptsPerTick = 8;
    private const int EventMobCapPerPlayer = 28;
    private const int DungeonSpawnAttemptsPerTick = 6;
    private const int DungeonMobCapPerPlayer = 3;
    private const float DayRoamerChance = 0.5f;
    private const float PassiveMobRarity = 0.1f;
    private const int PassiveMobCap = 24;
    private const float BoundNPCSpawnChance = 0.002f;

    private static int _timer;

    private static readonly Pool DayRoamers = new()
    {
        (ID.Slime, 4f), (ID.Pigeon, 0.2f), (ID.Gnome, 0.1f),
    };

    private static readonly Dictionary<BiomeType, Pool> DayByBiome = new()
    {
        [BiomeType.Grass] = Uniform(ID.Sheep),
        [BiomeType.Forest] = Uniform(ID.Hen, ID.Rooster, ID.Chick),
    };

    private static readonly Pool DesertNightMobs = Uniform(ID.SnareFlea);
    private static readonly Pool RaptureMobs = Uniform(ID.Lich);
    private static readonly Pool DungeonMobs = Uniform(ID.Sawblade, ID.Ballista, ID.Turret);
    private static readonly Pool NightMobs = Uniform(ID.SnareFlea, ID.Raider, ID.Congregant,
        ID.Acolyte, ID.Heretic, ID.Cultist, ID.Bear, ID.Watchdog, ID.TreeMimic, ID.Mannequin,
        ID.Vampire, ID.Sawblade, ID.Ballista, ID.Turret);

    public static void Update()
    {
        if (!Helper.IsHost() || Main.CreativeMode) return;

        if (++_timer < SpawnInterval) return;
        _timer = 0;

        bool dungeon = Save.Inst.current == GenType.Dungeon;
        bool eventActive = !dungeon && Save.Inst.weather is EnvironmentType.Rapture or EnvironmentType.NightBright;
        int cap = dungeon ? DungeonMobCapPerPlayer : eventActive ? EventMobCapPerPlayer : MobCapPerPlayer;
        int attempts = dungeon ? DungeonSpawnAttemptsPerTick : eventActive ? EventSpawnAttemptsPerTick : SpawnAttemptsPerTick;
        bool night = Save.Inst.weather is EnvironmentType.NightRainy or EnvironmentType.NightBright;

        if (!dungeon && CountMobs() >= Save.Inst.players.Count * cap) return;

        foreach (var player in Save.Inst.players)
        {
            if (player.Machine == null || player.controllerId == -1) continue;

            Vector3 pPos = player.Machine.transform.position;
            if (CountMobs(near: pPos, radius: Scene.LogicDistance) >= cap) continue;

            for (int i = 0; i < attempts; i++)
            {
                if (dungeon) TrySpawnDungeonMob(pPos);
                else TrySpawnGroup(pPos, night);
            }
        }
    }

    private static void TrySpawnDungeonMob(Vector3 playerPos)
    {
        Vector3Int pos = new(
            Mathf.RoundToInt(playerPos.x + Random.Range(-Scene.RenderDistance, Scene.RenderDistance)),
            0,
            Mathf.RoundToInt(playerPos.z + Random.Range(-Scene.RenderDistance, Scene.RenderDistance)));

        if (!World.IsInWorldBounds(pos) || NavMap.Get(pos) == NavMap.Air || !NavMap.IsAir(pos + Vector3Int.up))
            return;

        pos.y = 1;
        Entity.Spawn(PickWeighted(DungeonMobs), pos);
    }

    private static void TrySpawnGroup(Vector3 playerPos, bool night)
    {
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float dist = Random.Range(Scene.RenderDistance + 2, Scene.LogicDistance - 2);
        Vector3Int pos = Vector3Int.FloorToInt(
            playerPos + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * dist);

        if (!FindSurfacePosition(ref pos)) return;

        if (!night && Save.Inst.weather != EnvironmentType.Rapture && Random.value < BoundNPCSpawnChance)
        {
            Entity.Spawn(ID.BoundNPC, pos);
            return;
        }

        BiomeType biome = GenHelpBiome.GetBiomeType(pos.x, pos.z);
        Pool pool;
        if (Save.Inst.weather == EnvironmentType.Rapture) pool = RaptureMobs;
        else if (night) pool = biome == BiomeType.Desert ? DesertNightMobs : NightMobs;
        else if (Random.value < DayRoamerChance) pool = DayRoamers;
        else pool = DayByBiome.TryGetValue(biome, out Pool byBiome) ? byBiome : null;

        if (pool == null || pool.Count == 0) return;
        ID mobID = PickWeighted(pool);

        if (IsPassiveMob(mobID) &&
            (Random.value < PassiveMobRarity || CountMobs(m => IsPassiveMob(m.id)) >= PassiveMobCap))
            return;

        if (mobID != ID.Sheep)
        {
            Entity.Spawn(mobID, pos);
            return;
        }

        for (int herd = Random.Range(1, 3); herd > 0; herd--)
            Entity.Spawn(ID.Sheep, pos);
    }

    public static int CountMobs(Func<MobInfo, bool> where = null, Vector3 near = default, float radius = 0f) =>
        EntityDynamicLoad.ActiveEntities.Count(em =>
            em?.Info is MobInfo mob && mob is not PlayerInfo &&
            (radius <= 0f || Vector3.Distance(em.transform.position, near) <= radius) &&
            (where == null || where(mob)));

    private static bool IsPassiveMob(ID id) => id is ID.Sheep or ID.Hen or ID.Rooster or ID.Chick;

    private static Pool Uniform(params ID[] ids) => ids.Select(id => (id, 1f)).ToList();

    private static ID PickWeighted(Pool pool)
    {
        float roll = Random.value * pool.Sum(e => e.weight);
        foreach (var (id, weight) in pool)
            if ((roll -= weight) <= 0f) return id;
        return pool[^1].id;
    }

    private static bool FindSurfacePosition(ref Vector3Int pos)
    {
        pos.x = Mathf.Clamp(pos.x, 0, World.Inst.Bounds.x - 1);
        pos.z = Mathf.Clamp(pos.z, 0, World.Inst.Bounds.z - 1);

        for (pos.y = World.Inst.Bounds.y - 1; pos.y > 0; pos.y--)
            if (NavMap.Get(pos) == NavMap.Air && NavMap.Get(pos + Vector3Int.down) != NavMap.Air)
                return true;
        return false;
    }
}