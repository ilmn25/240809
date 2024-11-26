using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class EntityLoadStatic : MonoBehaviour
{
    public static EntityLoadStatic Instance { get; private set; }  
    
    private Vector3Int _currentChunkCoordinate;
    private List<EntityData> _chunkEntityList;
    private ChunkData _currentChunkData; 
    private GameObject _currentInstance;
    EntityHandler _currentEntityHandler;

    public static Dictionary<Vector3Int, (List<EntityData>, List<EntityHandler>)> _entityList = new Dictionary<Vector3Int, (List<EntityData>, List<EntityHandler>)>();
    public int ENTITY_DISTANCE = 4;

    void Awake()
    {
        Instance = this;
        WorldStatic.PlayerChunkTraverse += OnTraverse; 
    }

    private void OnTraverse()   
    {
        HandleUnload();
        HandleLoad();  
    }
 
    public void SaveAll()
    {
        foreach (var key in _entityList.Keys)
        {
            UpdateEntityList(key);
        }
    }
    
    void HandleUnload()
    {
        var keysToRemove = new List<Vector3Int>();
        foreach (var key in _entityList.Keys)
        {
            // Extract chunk coordinates from the key
            int chunkX = key.x, chunkZ = key.z;

            if (chunkX > WorldStatic._chunkPosition.x + ENTITY_DISTANCE * WorldStatic.CHUNKSIZE 
                || chunkX < WorldStatic._chunkPosition.x - ENTITY_DISTANCE * WorldStatic.CHUNKSIZE
                || chunkZ > WorldStatic._chunkPosition.z + ENTITY_DISTANCE * WorldStatic.CHUNKSIZE 
                || chunkZ < WorldStatic._chunkPosition.z - ENTITY_DISTANCE * WorldStatic.CHUNKSIZE)
            {
                UpdateEntityList(key);
                keysToRemove.Add(key);
            }
        }
        // Remove the marked keys from the dictionary
        foreach (var key in keysToRemove) _entityList.Remove(key);
    }

    void HandleLoad()
    {
        WorldStatic.Instance.HandleLoadWorldFile(0); 

        for (int x = -ENTITY_DISTANCE * WorldStatic.CHUNKSIZE; x <= ENTITY_DISTANCE * WorldStatic.CHUNKSIZE; x += WorldStatic.CHUNKSIZE)
        {
            for (int z = -ENTITY_DISTANCE * WorldStatic.CHUNKSIZE; z <= ENTITY_DISTANCE * WorldStatic.CHUNKSIZE; z += WorldStatic.CHUNKSIZE)
            {
                _currentChunkCoordinate = new Vector3Int(
                    Mathf.FloorToInt(WorldStatic._chunkPosition.x + x),
                    0,
                    Mathf.FloorToInt(WorldStatic._chunkPosition.z + z)
                );

                if (!_entityList.ContainsKey(_currentChunkCoordinate))
                {
                    _currentChunkData = WorldStatic.Instance.GetChunk(_currentChunkCoordinate); 
                    if  (_currentChunkData != null)
                    {
                        _chunkEntityList = _currentChunkData.StaticEntity;  
                        LoadChunkEntities(); 
                    }
                }  
            } 
        } 
    }
     
      
    public void UpdateEntityList(Vector3Int key)
    {
        foreach (EntityHandler entityHandler in _entityList[key].Item2)
        { 
            _entityList[key].Item1.Add(entityHandler.GetEntityData()); 
            EntityPoolStatic.Instance.ReturnObject(entityHandler.gameObject);   
        }
    }

    public void LoadChunkEntities()
    {  
        
        // Find the key once
        if (!_entityList.ContainsKey(_currentChunkCoordinate))
        {
            _entityList[_currentChunkCoordinate] = (_chunkEntityList, new List<EntityHandler>());
        }

        foreach (EntityData entityData in _chunkEntityList)
        { 
            _currentInstance = EntityPoolStatic.Instance.GetObject(entityData.ID);
            _currentInstance.transform.position = Lib.CombineVector(_currentChunkCoordinate, entityData.Position.ToVector3());

            _currentEntityHandler = _currentInstance.GetComponent<EntityHandler>();
            _entityList[_currentChunkCoordinate].Item2.Add(_currentEntityHandler);  
            _currentEntityHandler.Initialize(entityData, true);
        }
        _chunkEntityList.Clear();
    } 
}
 