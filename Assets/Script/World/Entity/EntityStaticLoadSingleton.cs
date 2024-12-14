using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class EntityStaticLoadSingleton : MonoBehaviour
{
    public static EntityStaticLoadSingleton Instance { get; private set; }  
     
    public static Dictionary<Vector3Int, (List<ChunkEntityData>, List<EntityMachine>)> _activeEntities = new Dictionary<Vector3Int, (List<ChunkEntityData>, List<EntityMachine>)>();

    void Awake()
    {
        Instance = this;
    }
 
    public static void ForgetEntity(EntityMachine entity) { }
    public static void InviteEntity(EntityMachine entity) { }
    
    public void UnloadWorld()
    {
        foreach (var key in _activeEntities.Keys)
        {
            UnloadEntitiesInChunk(key);
        }
        _activeEntities.Clear();
    }
      
    public void UnloadEntitiesInChunk(Vector3Int key)
    {
        foreach (EntityMachine entityHandler in _activeEntities[key].Item2)
        { 
            _activeEntities[key].Item1.Add(entityHandler.GetEntityData()); 
            EntityPoolSingleton.Instance.ReturnObject(entityHandler.gameObject);   
        }
    }

    public void LoadEntitiesInChunk(Vector3Int coordinate)
    {  
        EntityMachine currentEntityMachine;
        GameObject currentInstance;
        List<ChunkEntityData> activeEntities = WorldSingleton.World[coordinate].StaticEntity;
        // Find the key once
        if (!_activeEntities.ContainsKey(coordinate))
        {
            _activeEntities[coordinate] = (activeEntities, new List<EntityMachine>());
        }

        foreach (ChunkEntityData entityData in activeEntities)
        { 
            currentInstance = EntityPoolSingleton.Instance.GetObject(entityData.stringID);
            currentInstance.transform.position = coordinate + entityData.position.ToVector3Int() + new Vector3(0.5f, 0, 0.5f);

            currentEntityMachine = currentInstance.GetComponent<EntityMachine>();
            _activeEntities[coordinate].Item2.Add(currentEntityMachine);  
            currentEntityMachine.Initialize(entityData);
        }
        activeEntities.Clear();
    } 
}
 