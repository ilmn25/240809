using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class EntityStaticLoadSingleton : MonoBehaviour
{
    public static EntityStaticLoadSingleton Instance { get; private set; }  
    
    private List<ChunkEntityData> _chunkEntityList;
    private ChunkData _currentChunkData; 
    private GameObject _currentInstance;
    EntityMachine _currentEntityMachine;

    public static Dictionary<Vector3Int, (List<ChunkEntityData>, List<EntityMachine>)> _entityList = new Dictionary<Vector3Int, (List<ChunkEntityData>, List<EntityMachine>)>();
    private int ENTITY_DISTANCE = WorldSingleton.RENDER_DISTANCE;

    void Awake()
    {
        Instance = this;
        WorldSingleton.PlayerChunkTraverse += OnTraverse; 
    }

    public void OnTraverse()   
    {
        if (WorldSingleton._boolMap == null) return; // dont delete before boolmap load
        HandleUnload();
        // HandleLoad();  
    }
 
    public void SaveAll()
    {
        foreach (var key in _entityList.Keys)
        {
            UpdateEntityList(key);
        }
        _entityList.Clear();
    }
    
    void HandleUnload()
    {
        var removeList = new List<Vector3Int>();
        foreach (var coordinate in _entityList.Keys)
        {
            // Extract chunk coordinates from the key
            int chunkX = coordinate.x, chunkY = coordinate.y, chunkZ = coordinate.z;

            if (Math.Abs(chunkX - WorldSingleton._playerChunkPos.x) > ENTITY_DISTANCE * WorldSingleton.CHUNK_SIZE ||
                Math.Abs(chunkY - WorldSingleton._playerChunkPos.y) > ENTITY_DISTANCE * WorldSingleton.CHUNK_SIZE ||
                Math.Abs(chunkZ - WorldSingleton._playerChunkPos.z) > ENTITY_DISTANCE * WorldSingleton.CHUNK_SIZE)
            {
                UpdateEntityList(coordinate);
                removeList.Add(coordinate);
            }
        }
        // Remove the marked keys from the dictionary
        foreach (var coordinate in removeList) _entityList.Remove(coordinate);
    } 
     
      
    public void UpdateEntityList(Vector3Int key)
    {
        foreach (EntityMachine entityHandler in _entityList[key].Item2)
        { 
            _entityList[key].Item1.Add(entityHandler.GetEntityData()); 
            EntityPoolSingleton.Instance.ReturnObject(entityHandler.gameObject);   
        }
    }

    public void LoadChunkEntities(Vector3Int coordinate)
    {  
        _chunkEntityList = WorldSingleton.World[coordinate].StaticEntity;
        // Find the key once
        if (!_entityList.ContainsKey(coordinate))
        {
            _entityList[coordinate] = (_chunkEntityList, new List<EntityMachine>());
        }

        foreach (ChunkEntityData entityData in _chunkEntityList)
        { 
            _currentInstance = EntityPoolSingleton.Instance.GetObject(entityData.stringID);
            _currentInstance.transform.position = coordinate + entityData.position.ToVector3Int() + new Vector3(0.5f, 0, 0.5f);

            _currentEntityMachine = _currentInstance.GetComponent<EntityMachine>();
            _entityList[coordinate].Item2.Add(_currentEntityMachine);  
            _currentEntityMachine.Initialize(entityData);
        }
        _chunkEntityList.Clear();
    } 
}
 