using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class EntityStaticLoad
{
    public static readonly Dictionary<Vector3Int, (List<ChunkEntityData>, List<EntityMachine>)> ActiveEntities = new Dictionary<Vector3Int, (List<ChunkEntityData>, List<EntityMachine>)>();

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

    public static void LoadEntitiesInChunk(Vector3Int coordinate)
    {  
        EntityMachine currentEntityMachine;
        GameObject currentInstance;
        List<ChunkEntityData> activeEntities = World.Inst[coordinate].staticEntity;
        // Find the key once
        if (!ActiveEntities.ContainsKey(coordinate))
        {
            ActiveEntities[coordinate] = (activeEntities, new List<EntityMachine>());
        }

        foreach (ChunkEntityData entityData in activeEntities)
        { 
            currentInstance = ObjectPool.GetObject(entityData.stringID);
            currentInstance.transform.position = coordinate + entityData.position.ToVector3Int() + new Vector3(0.5f, 0, 0.5f);

            currentEntityMachine = currentInstance.GetComponent<EntityMachine>();
            ActiveEntities[coordinate].Item2.Add(currentEntityMachine);  
            currentEntityMachine.Initialize(entityData);
        }
        activeEntities.Clear();
    } 
}
 