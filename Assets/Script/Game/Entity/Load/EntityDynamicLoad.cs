using System.Collections.Generic;
using UnityEngine;

public class EntityDynamicLoad 
{

    private static List<EntityMachine> _activeEntities = new List<EntityMachine>();

    public static void Initialize()
    {
        Scene.PlayerChunkTraverse += ScanAndUnload; 
        Scene.PlayerChunkTraverse += ScanAndLoad; 
    }

    public static void ForgetEntity(EntityMachine entity) { _activeEntities.Remove(entity); }
    public static void InviteEntity(EntityMachine entity) { _activeEntities.Add(entity); }

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
                    World.Inst[entityChunkPosition].dynamicEntity.Add(entityMachine.GetEntityData());
                removeList.Add(entityMachine);
            }
        }
        foreach (var entityMachine in removeList) entityMachine.Delete();
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
                    LoadEntitiesInChunk(new Vector3Int(
                        Scene.PlayerChunkPosition.x + x * World.ChunkSize,
                        Scene.PlayerChunkPosition.y + y * World.ChunkSize,
                        Scene.PlayerChunkPosition.z + z * World.ChunkSize
                    ));
                }
            }
        } 
    } 
      
    public static void UnloadWorld()
    {
        List<EntityMachine> removeList = new List<EntityMachine>();
        Vector3Int entityChunkPosition;
        foreach (EntityMachine entityMachine in _activeEntities)
        {
            entityChunkPosition = World.GetChunkCoordinate(entityMachine.transform.position);
            if (World.IsInWorldBounds(entityChunkPosition))
                World.Inst[entityChunkPosition].dynamicEntity.Add(entityMachine.GetEntityData());
            removeList.Add(entityMachine);
        }
        foreach (var entityMachine in removeList) entityMachine.Delete();
    }

    private static void LoadEntitiesInChunk(Vector3Int chunkCoordinate)
    {
        Entity entity;
        EntityMachine currentEntityMachine;
        GameObject currentInstance = null;
        List<ChunkEntityData> chunkEntityList = World.Inst[chunkCoordinate].dynamicEntity;
        foreach (ChunkEntityData entityData in chunkEntityList)
        {   
            entity = Entity.Dictionary[entityData.stringID];
            if (entity.PrefabName == "item")
                currentInstance = ObjectPool.GetObject(entity.PrefabName);
            else                
                currentInstance = ObjectPool.GetObject(entity.PrefabName, entityData.stringID);
            
            currentInstance.transform.position = chunkCoordinate + entityData.position.ToVector3Int() + new Vector3(0.5f, 0.5f, 0.5f); 
            // Utility.Log(chunkCoordinate, entityData.position.ToVector3Int(), currentInstance.transform.position);
            currentEntityMachine = (EntityMachine)
                (currentInstance.GetComponent<EntityMachine>() ?? currentInstance.AddComponent(entity.Machine)); 
            _activeEntities.Add(currentEntityMachine); 
            currentEntityMachine.Initialize(entityData);
        }
        chunkEntityList.Clear();
    } 
}
 