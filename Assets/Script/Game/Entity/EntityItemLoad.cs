using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages active item entities with chunk-based persistence.
/// Items out of player logic range are saved to their current chunk's DynamicEntity list.
/// Items in player logic range are loaded from chunk DynamicEntity lists.
/// Worldgen ground items are placed in chunk DynamicEntity lists during generation.
/// </summary>
public class EntityItemLoad 
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
        ScanAndLoad();
    }

    /// <summary>Unload items far from all players, saving them to their current chunk.</summary>
    private static void ScanAndUnload()
    {
        List<EntityMachine> removeList = new List<EntityMachine>();
        foreach (var entityMachine in _activeEntities)
        { 
            Vector3Int entityChunkPosition = World.GetChunkCoordinate(entityMachine.transform.position);
            if (!Scene.AnyPlayerInChunkRange(entityChunkPosition, Scene.LogicDistance))
            {
                // Save to whichever chunk the item is currently in
                if (World.IsInWorldBounds(entityChunkPosition))
                    World.Inst[entityChunkPosition].DynamicEntity.Add(entityMachine.Info);
                removeList.Add(entityMachine);
            }
        }
        foreach (var entityMachine in removeList)
        {
            EntitySync.BroadcastEntityUnload(entityMachine.Info);
            entityMachine.Unload();
        }
    }

    /// <summary>Load items from chunk DynamicEntity lists in range of any player.</summary>
    private static void ScanAndLoad()
    {
        HashSet<Vector3Int> loaded = new HashSet<Vector3Int>();
        foreach (var player in Save.Inst.players)
        {
            if (player.Machine == null || player.controllerId == -1) continue;
            Vector3Int center = World.GetChunkCoordinate(player.Machine.transform.position);
            for (int x = -Scene.LogicRange; x <= Scene.LogicRange; x++)
            {
                for (int y = -Scene.LogicRange; y <= Scene.LogicRange; y++)
                {
                    for (int z = -Scene.LogicRange; z <= Scene.LogicRange; z++)
                    {
                        Vector3Int chunkCoordinate = new Vector3Int(
                            center.x + x * World.ChunkSize,
                            center.y + y * World.ChunkSize,
                            center.z + z * World.ChunkSize
                        );
                        if (!loaded.Add(chunkCoordinate)) continue;
                        LoadChunkItems(chunkCoordinate);
                    }
                }
            }
        }
    }

    /// <summary>Spawn all item Info objects stored in a single chunk's DynamicEntity list.</summary>
    private static void LoadChunkItems(Vector3Int chunkCoordinate)
    {
        Chunk chunk = World.Inst[chunkCoordinate];
        if (chunk == null || chunk == Chunk.Zero) return;
        foreach (Info info in chunk.DynamicEntity)
        {
            if (info is ItemInfo)
                Entity.SpawnFromInfo(info, true);
        }
        chunk.DynamicEntity.Clear();
    }

    /// <summary>Save all active items to their chunk's DynamicEntity, then unload everything.</summary>
    public static void UnloadWorld()
    {
        if (!Helper.IsHost()) return;

        List<EntityMachine> removeList = new List<EntityMachine>();
        foreach (EntityMachine entityMachine in _activeEntities)
        {
            Vector3Int chunkPos = World.GetChunkCoordinate(entityMachine.transform.position);
            if (World.IsInWorldBounds(chunkPos))
                World.Inst[chunkPos].DynamicEntity.Add(entityMachine.Info);
            removeList.Add(entityMachine);
        }
        foreach (var entityMachine in removeList)
        {
            EntitySync.BroadcastEntityUnload(entityMachine.Info);
            entityMachine.Unload();
        }
    }
}
