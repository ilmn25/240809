using UnityEngine;

/// <summary>Keeps sawblades and ballistas populated in the dungeon while a player is
/// inside it. Mobs aren't persisted to chunks, so this respawns them near players up
/// to a per-player cap (mirrors MobSpawner, but looks for brick floor tiles instead of
/// the open-world surface so it never drops them onto the ceiling).</summary>
public class DungeonMobSpawner
{
    private const int SpawnInterval = 240;
    private const int CapPerPlayer = 3;
    private const int SpawnAttemptsPerTick = 6;

    private static int _timer;

    public static void Update()
    {
        if (!Helper.IsHost()) return;
        if (Main.CreativeMode) return;
        if (Save.Inst.current != GenType.Dungeon) return;

        if (++_timer < SpawnInterval) return;
        _timer = 0;

        foreach (var player in Save.Inst.players)
        {
            if (player.Machine == null || player.controllerId == -1) continue;

            int nearby = 0;
            Vector3 pPos = player.Machine.transform.position;
            foreach (var em in EntityDynamicLoad.ActiveEntities)
            {
                if (em == null || em.Info is PlayerInfo) continue;
                if (Vector3.Distance(em.transform.position, pPos) <= Scene.LogicDistance) nearby++;
            }
            if (nearby >= CapPerPlayer) continue;

            for (int i = 0; i < SpawnAttemptsPerTick; i++)
                TrySpawnDungeonMob(pPos);
        }
    }

    private static void TrySpawnDungeonMob(Vector3 playerPos)
    {
        Vector3Int pos = new Vector3Int(
            Mathf.RoundToInt(playerPos.x + Random.Range(-Scene.RenderDistance, Scene.RenderDistance)),
            0,
            Mathf.RoundToInt(playerPos.z + Random.Range(-Scene.RenderDistance, Scene.RenderDistance)));

        if (!World.IsInWorldBounds(pos)) return;

        // A dungeon floor tile: solid brick at y=0 with air above (an interior
        // room). The y=0 floor check also keeps blades off the sealed ceiling.
        if (NavMap.Get(pos) == NavMap.Air || !NavMap.IsAir(pos + Vector3Int.up)) return;

        pos.y = 1;
        ID[] mobs = { ID.Sawblade, ID.Ballista, ID.Turret };
        Entity.Spawn(mobs[Random.Range(0, mobs.Length)], pos);
    }
}
