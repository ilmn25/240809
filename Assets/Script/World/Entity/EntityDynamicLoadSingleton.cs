using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class EntityDynamicLoadSingleton : MonoBehaviour
{
    public static EntityDynamicLoadSingleton Instance { get; private set; }  
    private List<ChunkEntityData> chunkEntityList;
    private ChunkData _currentChunkData; 
    private GameObject _currentInstance;
    EntityHandler _currentEntityHandler;

    public static List<EntityHandler> _entityList = new List<EntityHandler>();

    private int ENTITY_LOAD_DISTANCE = (WorldSingleton.RENDER_DISTANCE - 1) * WorldSingleton.CHUNK_SIZE;
    private int ENTITY_UNLOAD_DISTANCE = (WorldSingleton.RENDER_DISTANCE) * WorldSingleton.CHUNK_SIZE;
    
    void Awake()
    {
        Instance = this;
        WorldSingleton.PlayerChunkTraverse += OnTraverse; 
    }

    private void OnTraverse()   
    {
        //when move to new chunk 
        HandleUnload();
        // HandleLoad();  
    }
  
    
    void HandleUnload()
    {
        List<EntityHandler> removeList = new List<EntityHandler>();
        Vector3Int entityChunkPosition;
        foreach (var entityHandler in _entityList)
        { 
            entityChunkPosition = WorldSingleton.GetChunkCoordinate(entityHandler.transform.position);
            
            if (Math.Abs(entityChunkPosition.x - WorldSingleton._playerChunkPos.x) > ENTITY_UNLOAD_DISTANCE ||
                Math.Abs(entityChunkPosition.y - WorldSingleton._playerChunkPos.y) > ENTITY_UNLOAD_DISTANCE ||
                Math.Abs(entityChunkPosition.z - WorldSingleton._playerChunkPos.z) > ENTITY_UNLOAD_DISTANCE)
            {
                // Lib.Log(entityChunkPosition);
                if (WorldSingleton.Instance.IsInWorldBounds(entityChunkPosition))
                    WorldSingleton.Instance.GetChunk(entityChunkPosition).DynamicEntity.Add(entityHandler.GetEntityData());
                removeList.Add(entityHandler);
                EntityPoolSingleton.Instance.ReturnObject(entityHandler.gameObject); 
            }
        }
        foreach (var entityHandler in removeList) _entityList.Remove(entityHandler);
    }

    void HandleLoad()
    {
        WorldSingleton.Instance.HandleLoadWorldFile(0); 
        Vector3Int entityChunkPosition;

        for (int x = -ENTITY_LOAD_DISTANCE; x <= ENTITY_LOAD_DISTANCE; x += WorldSingleton.CHUNK_SIZE)
        {
            for (int y = -ENTITY_LOAD_DISTANCE; y <= ENTITY_LOAD_DISTANCE; y += WorldSingleton.CHUNK_SIZE)
            {
                for (int z = -ENTITY_LOAD_DISTANCE; z <= ENTITY_LOAD_DISTANCE; z += WorldSingleton.CHUNK_SIZE)
                {
                    entityChunkPosition = new Vector3Int(
                        Mathf.FloorToInt(WorldSingleton._playerChunkPos.x + x),
                        Mathf.FloorToInt(WorldSingleton._playerChunkPos.y + y),
                        Mathf.FloorToInt(WorldSingleton._playerChunkPos.z + z)
                    );
                    if (WorldSingleton.Instance.IsInWorldBounds(entityChunkPosition))
                    {
                        _currentChunkData = WorldSingleton.Instance.GetChunk(entityChunkPosition);
                        if (_currentChunkData != null)
                        {
                            LoadChunkEntities(entityChunkPosition);
                        }
                    }
 
                }
            } 
        } 
    }
     
      
    public void SaveAll()
    {
        Vector3Int entityChunkPosition;
        foreach (EntityHandler entityHandler in _entityList)
        {
            entityChunkPosition = WorldSingleton.GetChunkCoordinate(entityHandler.transform.position);
            if (WorldSingleton.Instance.IsInWorldBounds(entityChunkPosition))
                WorldSingleton.Instance.GetChunk(entityChunkPosition).DynamicEntity.Add(entityHandler.GetEntityData());
            EntityPoolSingleton.Instance.ReturnObject(entityHandler.gameObject);   
        }
        _entityList.Clear();
    }

    public void LoadChunkEntities(Vector3Int chunkCoordinate)
    {
        List<ChunkEntityData> chunkEntityList = WorldSingleton.World[chunkCoordinate].DynamicEntity;
        foreach (ChunkEntityData entityData in chunkEntityList)
        {   
            switch (EntitySingleton.dictionary[entityData.stringID].Type)
            {
                case EntityType.Item: 
                    _currentInstance = EntityPoolSingleton.Instance.GetObject("item"); 
                    _currentInstance.transform.position = chunkCoordinate + entityData.position.ToVector3Int() + new Vector3(0.5f, 1, 0.5f); 
        
                    _currentInstance.GetComponent<SpriteRenderer>().sprite = 
                        Resources.Load<Sprite>($"texture/sprite/{entityData.stringID}"); 
                    break;

                case EntityType.Rigid:
                    _currentInstance = EntityPoolSingleton.Instance.GetObject(entityData.stringID);
                    _currentInstance.transform.position = chunkCoordinate + entityData.position.ToVector3Int() + new Vector3(0.5f, 1, 0.5f); 
                    break;
            }
            
            _currentEntityHandler = _currentInstance.GetComponent<EntityHandler>();
            _entityList.Add(_currentEntityHandler); 
            _currentEntityHandler.Initialize(entityData, false);
        }
        chunkEntityList.Clear();
    } 
}
 