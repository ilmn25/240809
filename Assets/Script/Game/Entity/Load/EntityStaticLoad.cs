using System.Collections.Generic;
using UnityEngine;

public class EntityStaticLoad
{
    public static readonly Dictionary<Vector3Int, (List<Info>, List<EntityMachine>)> ActiveEntities = new Dictionary<Vector3Int, (List<Info>, List<EntityMachine>)>();

    public static void ForgetEntity(EntityMachine entityMachine, Entity entity)
    {
        ActiveEntities[World.GetChunkCoordinate(entityMachine.transform.position)].Item2.Remove(entityMachine);
        NavMap.SetEntity(entity, entityMachine.transform.position, false);
    }
    public static void InviteEntity(EntityMachine entityMachine, Entity entity) { // not done
        ActiveEntities[World.GetChunkCoordinate(entityMachine.transform.position)].Item2.Add(entityMachine);
        NavMap.SetEntity(entity, entityMachine.transform.position, false);
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
            ActiveEntities[key].Item1.Add(entityMachine.Info); 
            removeList.Add(entityMachine);
        }
        foreach (var entityMachine in removeList) entityMachine.Unload();
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

        foreach (Info info in activeEntities)
        {
            entity = Entity.Dictionary[info.stringID];
            currentInstance = ObjectPool.GetObject(entity.PrefabName, info.stringID);
            currentInstance.transform.position = info.position;
            currentEntityMachine = (EntityMachine)
                (currentInstance.GetComponent<EntityMachine>() ?? currentInstance.AddComponent(entity.Machine));
            ActiveEntities[chunkCoordinate].Item2.Add(currentEntityMachine);  
            currentEntityMachine.Initialize(info);
        }
        activeEntities.Clear();
    } 
}
 