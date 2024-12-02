using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class EntityStaticLoadSingleton : MonoBehaviour
{
    public static EntityStaticLoadSingleton Instance { get; private set; }  
    
    private Vector3Int _currentChunkCoordinate;
    private List<EntityData> _chunkEntityList;
    private ChunkData _currentChunkData; 
    private GameObject _currentInstance;
    EntityHandler _currentEntityHandler;

    public static Dictionary<Vector3Int, (List<EntityData>, List<EntityHandler>)> _entityList = new Dictionary<Vector3Int, (List<EntityData>, List<EntityHandler>)>();
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
        var removeList = new List<Vector3Int>();
        foreach (var coordinate in _entityList.Keys)
        {
            // Extract chunk coordinates from the key
            int chunkX = coordinate.x, chunkY = coordinate.y, chunkZ = coordinate.z;

            if (chunkX > WorldSingleton._playerChunkPos.x + ENTITY_DISTANCE * WorldSingleton.CHUNK_SIZE 
                || chunkX < WorldSingleton._playerChunkPos.x - ENTITY_DISTANCE * WorldSingleton.CHUNK_SIZE
                || chunkY > WorldSingleton._playerChunkPos.y + ENTITY_DISTANCE * WorldSingleton.CHUNK_SIZE 
                || chunkY < WorldSingleton._playerChunkPos.y - ENTITY_DISTANCE * WorldSingleton.CHUNK_SIZE
                || chunkZ > WorldSingleton._playerChunkPos.z + ENTITY_DISTANCE * WorldSingleton.CHUNK_SIZE 
                || chunkZ < WorldSingleton._playerChunkPos.z - ENTITY_DISTANCE * WorldSingleton.CHUNK_SIZE)
            {
                UpdateEntityList(coordinate);
                removeList.Add(coordinate);
            }
        }
        // Remove the marked keys from the dictionary
        foreach (var coordinate in removeList) _entityList.Remove(coordinate);
    }

    void HandleLoad()
    { 

        for (int x = -ENTITY_DISTANCE * WorldSingleton.CHUNK_SIZE; x <= ENTITY_DISTANCE * WorldSingleton.CHUNK_SIZE; x += WorldSingleton.CHUNK_SIZE)
        {
            for (int y = -ENTITY_DISTANCE * WorldSingleton.CHUNK_SIZE; y <= ENTITY_DISTANCE * WorldSingleton.CHUNK_SIZE; y += WorldSingleton.CHUNK_SIZE)
            {
                for (int z = -ENTITY_DISTANCE * WorldSingleton.CHUNK_SIZE; z <= ENTITY_DISTANCE * WorldSingleton.CHUNK_SIZE; z += WorldSingleton.CHUNK_SIZE)
                {
                    _currentChunkCoordinate = new Vector3Int(
                        Mathf.FloorToInt(WorldSingleton._playerChunkPos.x + x),
                        Mathf.FloorToInt(WorldSingleton._playerChunkPos.y + y),
                        Mathf.FloorToInt(WorldSingleton._playerChunkPos.z + z)
                    );

                    if (!_entityList.ContainsKey(_currentChunkCoordinate))
                    {
                        _currentChunkData = WorldSingleton.Instance.GetChunk(_currentChunkCoordinate); 
                        if  (_currentChunkData != null)
                        {
                            _chunkEntityList = _currentChunkData.StaticEntity;  
                            LoadChunkEntities(); 
                        }
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
            EntityPoolSingleton.Instance.ReturnObject(entityHandler.gameObject);   
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
            _currentInstance = EntityPoolSingleton.Instance.GetObject(entityData.ID);
            _currentInstance.transform.position = Lib.CombineVector(_currentChunkCoordinate, entityData.Position.ToVector3());

            _currentEntityHandler = _currentInstance.GetComponent<EntityHandler>();
            _entityList[_currentChunkCoordinate].Item2.Add(_currentEntityHandler);  
            _currentEntityHandler.Initialize(entityData, true);
        }
        _chunkEntityList.Clear();
    } 
}
 