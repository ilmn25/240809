using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages active dynamic entities (mobs, items) in a single global list.
/// Entities that leave player logic range are despawned (not saved back to chunks).
/// New entities are spawned by MobSpawner or player actions — never loaded from world data.
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
    
    private static void ScanAndUnload()
    {
        List<EntityMachine> removeList = new List<EntityMachine>();
        foreach (var entityMachine in _activeEntities)
        { 
            Vector3Int entityChunkPosition = World.GetChunkCoordinate(entityMachine.transform.position);
            if (!AnyPlayerInChunkRange(entityChunkPosition, Scene.LogicDistance))
            {
                removeList.Add(entityMachine);
            }
        }
        foreach (var entityMachine in removeList)
        {
            EntitySync.BroadcastEntityUnload(entityMachine.Info);
            entityMachine.Unload();
        }
    }

    public static bool AnyPlayerInChunkRange(Vector3 chunkCoord, float distance)
    {
        foreach (var player in Save.Inst.players)
        {
            if (player.Machine == null || player.controllerId == -1) continue;
            Vector3Int playerChunk = World.GetChunkCoordinate(player.Machine.transform.position);
            if (chunkCoord.x >= playerChunk.x - distance && chunkCoord.x <= playerChunk.x + distance + 1 &&
                chunkCoord.y >= playerChunk.y - distance && chunkCoord.y <= playerChunk.y + distance + 1 &&
                chunkCoord.z >= playerChunk.z - distance && chunkCoord.z <= playerChunk.z + distance + 1)
                return true;
        }
        return false;
    }

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
 