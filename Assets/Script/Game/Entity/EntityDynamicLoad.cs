using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages active mob entities. Mobs are never persisted to chunks — MobSpawner handles
/// spawning/respawning. Entities out of player logic range are simply discarded.
/// </summary>
public class EntityDynamicLoad 
{
    private static readonly List<EntityMachine> _activeEntities = new List<EntityMachine>();
    public static List<EntityMachine> ActiveEntities => _activeEntities;

    public static void ForgetEntity(EntityMachine entity)
    {
        if (entity == null) return;
        while (_activeEntities.Remove(entity)) { }
    }

    public static void InviteEntity(EntityMachine entity)
    {
        if (entity == null) return;
        if (_activeEntities.Contains(entity)) return;
        _activeEntities.Add(entity);
    }

    public static void OnChunkTraverse()
    {
        if (!Helper.IsHost()) return;
        ScanAndUnload();
    }

    /// <summary>Discard mobs far from all players (MobSpawner will respawn them).</summary>
    private static void ScanAndUnload()
    {
        List<EntityMachine> removeList = new List<EntityMachine>();
        foreach (var entityMachine in _activeEntities)
        { 
            Vector3Int entityChunkPosition = World.GetChunkCoordinate(entityMachine.transform.position);
            if (!Scene.AnyPlayerInChunkRange(entityChunkPosition, Scene.LogicDistance))
                removeList.Add(entityMachine);
        }
        foreach (var entityMachine in removeList)
        {
            EntitySync.BroadcastEntityUnload(entityMachine.Info);
            entityMachine.Unload();
        }
    }

    /// <summary>Unload all mobs — no persistence (MobSpawner handles respawns).</summary>
    public static void UnloadWorld()
    {
        if (!Helper.IsHost()) return;

        List<EntityMachine> removeList = new List<EntityMachine>(_activeEntities);
        foreach (var entityMachine in removeList)
        {
            EntitySync.BroadcastEntityUnload(entityMachine.Info);
            entityMachine.Unload();
        }
    }
}
 