using System;
using System.Collections.Generic;
using UnityEngine;

public class EntityDynamicLoad 
{

    private static List<EntityMachine> _activeEntities = new List<EntityMachine>();

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
    
    private static void ScanAndUnload()
    {
        List<EntityMachine> removeList = new List<EntityMachine>();
        Vector3Int entityChunkPosition;
        foreach (var entityMachine in _activeEntities)
        { 
            entityChunkPosition = World.GetChunkCoordinate(entityMachine.transform.position);
            
            if (!Scene.InPlayerChunkRange(entityChunkPosition, Scene.LogicDistance))
            {
                if (World.IsInWorldBounds(entityChunkPosition))
                    World.Inst[entityChunkPosition].DynamicEntity.Add(entityMachine.Info);
                removeList.Add(entityMachine);
            }
        }
        foreach (var entityMachine in removeList) entityMachine.Unload();
    }

    private static void ScanAndLoad()
    {
        // Collect chunk coordinates within render distance
        for (int x = -Scene.LogicRange; x <= Scene.LogicRange; x++)
        {
            for (int y = -Scene.LogicRange; y <= Scene.LogicRange; y++)
            {
                for (int z = -Scene.LogicRange; z <= Scene.LogicRange; z++)
                {
                    Vector3Int chunkCoordinate = new Vector3Int(
                        Scene.PlayerChunkPosition.x + x * World.ChunkSize,
                        Scene.PlayerChunkPosition.y + y * World.ChunkSize,
                        Scene.PlayerChunkPosition.z + z * World.ChunkSize
                    );
                    NavMap.SetChunk(chunkCoordinate);
                    LoadEntitiesInChunk(chunkCoordinate); 
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
        foreach (var entityMachine in removeList) entityMachine.Unload();
    }

    private static void LoadEntitiesInChunk(Vector3Int chunkCoordinate)
    {
        List<Info> chunkEntityList = World.Inst[chunkCoordinate].DynamicEntity; 
        foreach (Info info in chunkEntityList)
            Entity.SpawnFromInfo(info);
        World.Inst[chunkCoordinate].DynamicEntity.Clear(); 
    } 
}
 