using System.Collections.Generic;
using UnityEngine;

public class EntityStaticLoad
{
    public static readonly Dictionary<Vector3Int, (List<Info>, List<EntityMachine>)> ActiveEntities = new Dictionary<Vector3Int, (List<Info>, List<EntityMachine>)>();

    public static void ForgetEntity(EntityMachine entity)
    {
        ActiveEntities[World.GetChunkCoordinate(entity.transform.position)].Item2.Remove(entity);
    }
    public static void InviteEntity(EntityMachine entity) { // not done
        ActiveEntities[World.GetChunkCoordinate(entity.transform.position)].Item2.Add(entity);
    }
    
    public static void UnloadWorld()
    {
        foreach (var key in ActiveEntities.Keys)
        {
            UnloadEntitiesInChunk(key);
        }
        ActiveEntities.Clear();
    }
      
    public static void UnloadEntitiesInChunk(Vector3Int key)
    {
        List<EntityMachine> removeList = new List<EntityMachine>();
        foreach (EntityMachine entityMachine in ActiveEntities[key].Item2)
        { 
            ActiveEntities[key].Item1.Add(entityMachine.GetEntityData()); 
            removeList.Add(entityMachine);
        }
        foreach (var entityMachine in removeList) entityMachine.Delete();
    }

    public static void LoadEntitiesInChunk(Vector3Int chunkCoordinate)
    {  
        Entity entity;
        EntityMachine currentEntityMachine;
        GameObject currentInstance;
        List<Info> activeEntities = World.Inst[chunkCoordinate].StaticEntity;
        // Find the key once
        if (!ActiveEntities.ContainsKey(chunkCoordinate))
        {
            ActiveEntities[chunkCoordinate] = (activeEntities, new List<EntityMachine>());
        }

        foreach (Info entityData in activeEntities)
        {
            entity = Entity.Dictionary[entityData.stringID];
            currentInstance = ObjectPool.GetObject(entity.PrefabName, entityData.stringID);
            
            if (entity.PrefabName == "block")
                currentInstance.transform.position = entityData.position;
            else
                currentInstance.transform.position = entityData.position + new Vector3(0.5f, 0, 0.5f);
            
            currentEntityMachine = (EntityMachine)
                (currentInstance.GetComponent<EntityMachine>() ?? currentInstance.AddComponent(entity.Machine));
            ActiveEntities[chunkCoordinate].Item2.Add(currentEntityMachine);  
            currentEntityMachine.Initialize(entityData);
        }
        activeEntities.Clear();
    } 
}
 