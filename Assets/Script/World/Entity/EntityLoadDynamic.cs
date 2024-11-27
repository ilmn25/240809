using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class EntityLoadDynamic : MonoBehaviour
{
    public static EntityLoadDynamic Instance { get; private set; }  
    public static event Action UpdateEntityListKey; 
    private Vector3Int _currentChunkCoordinate;
    private List<EntityData> chunkEntityList;
    private ChunkData _currentChunkData; 
    private GameObject _currentInstance;
    EntityHandler _currentEntityHandler;

    public static List<EntityHandler> _entityList = new List<EntityHandler>();
    public int ENTITY_DISTANCE = 4;

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
        var keysToRemove = new List<Vector3Int>();
        UpdateEntityListKey?.Invoke();

        foreach (var entityHandler in _entityList)
        { 
            if (Vector3.Distance(entityHandler.transform.position, Game.Player.transform.position) > 60) { 
                Lib.Log(WorldStatic.GetChunkCoordinate(entityHandler.transform.position));
                WorldStatic.Instance.GetChunk(WorldStatic.GetChunkCoordinate(entityHandler.transform.position)).DynamicEntity.Add(entityHandler.GetEntityData());
                EntityPoolStatic.Instance.ReturnObject(entityHandler.gameObject); 
            }
        }
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
 
                _currentChunkData = WorldStatic.Instance.GetChunk(_currentChunkCoordinate); 
                if  (_currentChunkData != null)
                {
                    LoadChunkEntities(_currentChunkData.DynamicEntity); 
                }
            } 
        } 
    }
     
      
    public void SaveAll()
    {
        foreach (EntityHandler entityHandler in _entityList)
        { 
            WorldStatic.Instance.GetChunk(WorldStatic.GetChunkCoordinate(entityHandler.transform.position)).DynamicEntity.Add(entityHandler.GetEntityData());
            EntityPoolStatic.Instance.ReturnObject(entityHandler.gameObject);   
        }
    }

    public void LoadChunkEntities(List<EntityData> chunkEntityList)
    { 
        foreach (EntityData entityData in chunkEntityList)
        {   
            switch (entityData.Type)
            {
                case EntityType.Item: 
                    _currentInstance = EntityPoolStatic.Instance.GetObject("item"); 
                    entityData.Position = new SerializableVector3(Lib.CombineVector(_currentChunkCoordinate, entityData.Position.ToVector3()));
                    _currentInstance.transform.position = entityData.Position.ToVector3(); 
        
                    _currentInstance.GetComponent<SpriteRenderer>().sprite = 
                        Resources.Load<Sprite>($"texture/sprite/{entityData.ID}"); 
                    break;

                case EntityType.Rigid:
                    _currentInstance = EntityPoolStatic.Instance.GetObject(entityData.ID);
                    _currentInstance.transform.position = Lib.AddToVector(Lib.CombineVector(_currentChunkCoordinate, entityData.Position.ToVector3()), 0, 0.5f, 0);
                    break;
            }
            
            _currentEntityHandler = _currentInstance.GetComponent<EntityHandler>();
            _entityList.Add(_currentEntityHandler); 
            _currentEntityHandler.Initialize(entityData, false);
        }
        chunkEntityList.Clear();
    } 
}
 