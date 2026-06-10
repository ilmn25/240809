using System;
using System.Collections.Generic;
using UnityEngine;

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

    private static bool AnyPlayerInChunkRange(Vector3 chunkCoord, float distance)
    {
        foreach (var player in Save.Inst.players)
        {
            if (player.Machine == null || player.ownerId == "-1") continue;
            Vector3Int playerChunk = World.GetChunkCoordinate(player.Machine.transform.position);
            if (chunkCoord.x >= playerChunk.x - distance && chunkCoord.x <= playerChunk.x + distance + 1 &&
                chunkCoord.y >= playerChunk.y - distance && chunkCoord.y <= playerChunk.y + distance + 1 &&
                chunkCoord.z >= playerChunk.z - distance && chunkCoord.z <= playerChunk.z + distance + 1)
                return true;
        }
        return false;
    }

    public static void OnChunkTraverse()
    {
        if (!Helper.IsHost()) return;
        ScanAndUnload();
        ScanAndLoad();
    }
    
    private static void ScanAndUnload()
    {
        List<EntityMachine> removeList = new List<EntityMachine>();
        Vector3Int entityChunkPosition;
        foreach (var entityMachine in _activeEntities)
        { 
            entityChunkPosition = World.GetChunkCoordinate(entityMachine.transform.position);
            
            if (!AnyPlayerInChunkRange(entityChunkPosition, Scene.LogicDistance))
            {
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

    private static void ScanAndLoad()
    {
        HashSet<Vector3Int> loaded = new HashSet<Vector3Int>();
        foreach (var player in Save.Inst.players)
        {
            if (player.Machine == null || player.ownerId == "-1") continue;
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
                        if (NavMap.SetChunk(chunkCoordinate))
                            NavMapSync.BroadcastChunk(chunkCoordinate);
                        LoadEntitiesInChunk(chunkCoordinate);
                    }
                }
            }
        }
    } 
      
    public static void UnloadWorld()
    {
        if (!Helper.IsHost()) return;

        List<EntityMachine> removeList = new List<EntityMachine>();
        Vector3Int entityChunkPosition;
        foreach (EntityMachine entityMachine in _activeEntities)
        {
            entityChunkPosition = World.GetChunkCoordinate(entityMachine.transform.position);
            if (World.IsInWorldBounds(entityChunkPosition))
                World.Inst[entityChunkPosition].DynamicEntity.Add(entityMachine.Info);
            removeList.Add(entityMachine);
        }
        foreach (var entityMachine in removeList)
        {
            EntitySync.BroadcastEntityUnload(entityMachine.Info);
            entityMachine.Unload();
        }
    }

    private static void LoadEntitiesInChunk(Vector3Int chunkCoordinate)
    {
        List<Info> chunkEntityList = World.Inst[chunkCoordinate].DynamicEntity; 
        foreach (Info info in chunkEntityList)
            Entity.SpawnFromInfo(info, true);
        World.Inst[chunkCoordinate].DynamicEntity.Clear(); 
    } 
}
 