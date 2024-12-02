using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class EntityLoadDynamic : MonoBehaviour
{
    public static EntityLoadDynamic Instance { get; private set; }  
    private List<EntityData> chunkEntityList;
    private ChunkData _currentChunkData; 
    private GameObject _currentInstance;
    EntityHandler _currentEntityHandler;

    public static List<EntityHandler> _entityList = new List<EntityHandler>();

    private int ENTITY_LOAD_DISTANCE = (WorldStatic.RENDER_DISTANCE - 1) * WorldStatic.CHUNK_SIZE;
    private int ENTITY_UNLOAD_DISTANCE = (WorldStatic.RENDER_DISTANCE) * WorldStatic.CHUNK_SIZE;
    
    void Awake()
    {
        Instance = this;
        WorldStatic.PlayerChunkTraverse += OnTraverse; 
    }

    private void OnTraverse()   
    {
        //when move to new chunk 
        HandleUnload();
        HandleLoad();  
    }
  
    
    void HandleUnload()
    {
        Vector3Int entityChunkPosition;
        foreach (var entityHandler in _entityList)
        { 
            entityChunkPosition = WorldStatic.GetChunkCoordinate(entityHandler.transform.position);
            
            if (entityChunkPosition.x > WorldStatic._playerChunkPos.x + ENTITY_UNLOAD_DISTANCE || entityChunkPosition.x < WorldStatic._playerChunkPos.x - ENTITY_UNLOAD_DISTANCE ||
                entityChunkPosition.y > WorldStatic._playerChunkPos.y + ENTITY_UNLOAD_DISTANCE || entityChunkPosition.y < WorldStatic._playerChunkPos.y - ENTITY_UNLOAD_DISTANCE ||
                entityChunkPosition.z > WorldStatic._playerChunkPos.z + ENTITY_UNLOAD_DISTANCE || entityChunkPosition.z < WorldStatic._playerChunkPos.z - ENTITY_UNLOAD_DISTANCE)
            {
                // Lib.Log(entityChunkPosition);
                if (WorldStatic.Instance.IsInWorldBounds(entityChunkPosition))
                    WorldStatic.Instance.GetChunk(entityChunkPosition).DynamicEntity.Add(entityHandler.GetEntityData());
                
                EntityPoolStatic.Instance.ReturnObject(entityHandler.gameObject); 
            }
        }
    }

    void HandleLoad()
    {
        WorldStatic.Instance.HandleLoadWorldFile(0); 
        Vector3Int entityChunkPosition;

        for (int x = -ENTITY_LOAD_DISTANCE; x <= ENTITY_LOAD_DISTANCE; x += WorldStatic.CHUNK_SIZE)
        {
            for (int y = -ENTITY_LOAD_DISTANCE; y <= ENTITY_LOAD_DISTANCE; y += WorldStatic.CHUNK_SIZE)
            {
                for (int z = -ENTITY_LOAD_DISTANCE; z <= ENTITY_LOAD_DISTANCE; z += WorldStatic.CHUNK_SIZE)
                {
                    entityChunkPosition = new Vector3Int(
                        Mathf.FloorToInt(WorldStatic._playerChunkPos.x + x),
                        Mathf.FloorToInt(WorldStatic._playerChunkPos.y + y),
                        Mathf.FloorToInt(WorldStatic._playerChunkPos.z + z)
                    );
                    if (WorldStatic.Instance.IsInWorldBounds(entityChunkPosition))
                    {
                        _currentChunkData = WorldStatic.Instance.GetChunk(entityChunkPosition);
                        if (_currentChunkData != null)
                        {
                            LoadChunkEntities(_currentChunkData.DynamicEntity, entityChunkPosition);
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
            entityChunkPosition = WorldStatic.GetChunkCoordinate(entityHandler.transform.position);
            if (WorldStatic.Instance.IsInWorldBounds(entityChunkPosition))
                WorldStatic.Instance.GetChunk(entityChunkPosition).DynamicEntity.Add(entityHandler.GetEntityData());
            EntityPoolStatic.Instance.ReturnObject(entityHandler.gameObject);   
        }
    }

    public void LoadChunkEntities(List<EntityData> chunkEntityList, Vector3Int chunkCoordinate)
    { 
        foreach (EntityData entityData in chunkEntityList)
        {   
            switch (entityData.Type)
            {
                case EntityType.Item: 
                    _currentInstance = EntityPoolStatic.Instance.GetObject("item"); 
                    entityData.Position = new SerializableVector3(Lib.CombineVector(chunkCoordinate, entityData.Position.ToVector3()));
                    _currentInstance.transform.position = entityData.Position.ToVector3(); 
        
                    _currentInstance.GetComponent<SpriteRenderer>().sprite = 
                        Resources.Load<Sprite>($"texture/sprite/{entityData.ID}"); 
                    break;

                case EntityType.Rigid:
                    _currentInstance = EntityPoolStatic.Instance.GetObject(entityData.ID);
                    _currentInstance.transform.position = Lib.AddToVector(Lib.CombineVector(chunkCoordinate, entityData.Position.ToVector3()), 0, 2f, 0);
                    break;
            }
            
            _currentEntityHandler = _currentInstance.GetComponent<EntityHandler>();
            _entityList.Add(_currentEntityHandler); 
            _currentEntityHandler.Initialize(entityData, false);
        }
        chunkEntityList.Clear();
    } 
}
 