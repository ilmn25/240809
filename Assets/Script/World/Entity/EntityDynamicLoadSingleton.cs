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
    EntityMachine _currentEntityMachine;

    public static List<EntityMachine> _entityList = new List<EntityMachine>();

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
        List<EntityMachine> removeList = new List<EntityMachine>();
        Vector3Int entityChunkPosition;
        foreach (var entityMachine in _entityList)
        { 
            entityChunkPosition = WorldSingleton.GetChunkCoordinate(entityMachine.transform.position);
            
            if (Math.Abs(entityChunkPosition.x - WorldSingleton._playerChunkPos.x) > ENTITY_UNLOAD_DISTANCE ||
                Math.Abs(entityChunkPosition.y - WorldSingleton._playerChunkPos.y) > ENTITY_UNLOAD_DISTANCE ||
                Math.Abs(entityChunkPosition.z - WorldSingleton._playerChunkPos.z) > ENTITY_UNLOAD_DISTANCE)
            {
                // Lib.Log(entityChunkPosition);
                if (WorldSingleton.Instance.IsInWorldBounds(entityChunkPosition))
                    WorldSingleton.Instance.GetChunk(entityChunkPosition).DynamicEntity.Add(entityMachine.GetEntityData());
                removeList.Add(entityMachine);
                EntityPoolSingleton.Instance.ReturnObject(entityMachine.gameObject); 
            }
        }
        foreach (var entityMachine in removeList) _entityList.Remove(entityMachine);
    }
 
     
      
    public void SaveAll()
    {
        Vector3Int entityChunkPosition;
        foreach (EntityMachine entityHandler in _entityList)
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
            
            _currentEntityMachine = _currentInstance.GetComponent<EntityMachine>();
            _entityList.Add(_currentEntityMachine); 
            _currentEntityMachine.Initialize(entityData);
        }
        chunkEntityList.Clear();
    } 
}
 