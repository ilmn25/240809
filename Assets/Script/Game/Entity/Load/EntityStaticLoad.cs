using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class EntityStaticLoad
{
    public static Dictionary<Vector3Int, (List<ChunkEntityData>, List<EntityMachine>)> _activeEntities = new Dictionary<Vector3Int, (List<ChunkEntityData>, List<EntityMachine>)>();

    public static void ForgetEntity(EntityMachine entity)
    {
        _activeEntities[World.GetChunkCoordinate(entity.transform.position)].Item2.Remove(entity);
    }
    public static void InviteEntity(EntityMachine entity) { // not done
        _activeEntities[World.GetChunkCoordinate(entity.transform.position)].Item2.Add(entity);
        
    }
    
    public static void UnloadWorld()
    {
        foreach (var key in _activeEntities.Keys)
        {
            UnloadEntitiesInChunk(key);
        }
        _activeEntities.Clear();
    }
      
    public static void UnloadEntitiesInChunk(Vector3Int key)
    {
        foreach (EntityMachine entityHandler in _activeEntities[key].Item2)
        { 
            _activeEntities[key].Item1.Add(entityHandler.GetEntityData()); 
            ObjectPool.ReturnObject(entityHandler.gameObject);   
        }
    }

    public static void LoadEntitiesInChunk(Vector3Int coordinate)
    {  
        EntityMachine currentEntityMachine;
        GameObject currentInstance;
        List<ChunkEntityData> activeEntities = World.Inst[coordinate].staticEntity;
        // Find the key once
        if (!_activeEntities.ContainsKey(coordinate))
        {
            _activeEntities[coordinate] = (activeEntities, new List<EntityMachine>());
        }

        foreach (ChunkEntityData entityData in activeEntities)
        { 
            currentInstance = ObjectPool.GetObject(entityData.stringID);
            currentInstance.transform.position = coordinate + entityData.position.ToVector3Int() + new Vector3(0.5f, 0, 0.5f);

            currentEntityMachine = currentInstance.GetComponent<EntityMachine>();
            _activeEntities[coordinate].Item2.Add(currentEntityMachine);  
            currentEntityMachine.Initialize(entityData);
        }
        activeEntities.Clear();
    } 
}
 