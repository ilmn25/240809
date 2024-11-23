using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class EntityLoadStatic : MonoBehaviour
{
    public static EntityLoadStatic Instance { get; private set; }  
    public static event Action UpdateEntityListKey; 
    private int _chunkSize;
    private Vector3Int _currentChunkCoordinate;
    private List<EntityData> _chunkEntityList;
    private ChunkData _currentChunkData; 
    private GameObject _currentInstance;

    public static Dictionary<Vector3Int, (List<EntityData>, List<EntityDataHandler>)> _entityList = new Dictionary<Vector3Int, (List<EntityData>, List<EntityDataHandler>)>();
    public int ENTITY_DISTANCE = 2;

    void Awake()
    {
        Instance = this;
        WorldStatic.PlayerChunkPositionUpdate += HandleChunkEntityTraverse; 
        _chunkSize = WorldStatic.CHUNKSIZE;
    }


    public void UpdateEntityList(Vector3Int key)
    {
        _entityList[key].Item1.Clear();

        foreach (EntityDataHandler entityHandler in _entityList[key].Item2)
        { 
            _entityList[key].Item1.Add(entityHandler.GetUpdatedEntity());
            Destroy(entityHandler.gameObject);  
        }
    }
    
    public void SaveAllEntities()
    {
        UpdateEntityListKey?.Invoke();
        foreach (var key in _entityList.Keys)
        {
            UpdateEntityList(key);
        }
    }

    private void HandleChunkEntityTraverse()
    {
        Vector3 currentChunkPosition = WorldStatic._chunkPosition; 
        HandleUnload();
        HandleLoad(); 
        void HandleUnload()
        {
            var keysToRemove = new List<Vector3Int>();
            UpdateEntityListKey?.Invoke();

            foreach (var key in _entityList.Keys)
            {
                // Extract chunk coordinates from the key
                int chunkX = key.x, chunkZ = key.z;

                if (chunkX > currentChunkPosition.x + ENTITY_DISTANCE * _chunkSize 
                    || chunkX < currentChunkPosition.x - ENTITY_DISTANCE * _chunkSize
                    || chunkZ > currentChunkPosition.z + ENTITY_DISTANCE * _chunkSize 
                    || chunkZ < currentChunkPosition.z - ENTITY_DISTANCE * _chunkSize)
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

            for (int x = -ENTITY_DISTANCE * _chunkSize; x <= ENTITY_DISTANCE * _chunkSize; x += _chunkSize)
            {
                for (int z = -ENTITY_DISTANCE * _chunkSize; z <= ENTITY_DISTANCE * _chunkSize; z += _chunkSize)
                {
                    _currentChunkCoordinate = new Vector3Int(
                        Mathf.FloorToInt(currentChunkPosition.x + x),
                        0,
                        Mathf.FloorToInt(currentChunkPosition.z + z)
                    );

                    if (!_entityList.ContainsKey(_currentChunkCoordinate))
                    {
                        _currentChunkData = WorldStatic.Instance.GetChunk(_currentChunkCoordinate); 
                        if  (_currentChunkData != null)
                        {
                            _chunkEntityList = _currentChunkData.Entity;  
                            LoadChunkEntities(); 
                        }
                    }  
                } 
            } 
        }
    }
      
    
    public void LoadChunkEntities()
    { 
        EntityDataHandler entityDataHandler;

        // Find the key once
        if (!_entityList.ContainsKey(_currentChunkCoordinate))
        {
            _entityList[_currentChunkCoordinate] = (_chunkEntityList, new List<EntityDataHandler>());
        }

        foreach (EntityData entity in _chunkEntityList)
        { 
            switch (entity.Type)
            {
                case EntityType.Item:
                    entity.Position = new SerializableVector3(Lib.CombineVector(_currentChunkCoordinate, entity.Position.ToVector3()));
                    ItemLoadStatic.Instance.SpawnItem(entity);
                    break;

                case EntityType.Static:
                    InstantiatePrefab(entity);
                    _currentInstance.transform.position = Lib.CombineVector(_currentChunkCoordinate, entity.Position.ToVector3());
                    break;

                case EntityType.Rigid:
                    InstantiatePrefab(entity);
                    _currentInstance.transform.position = Lib.AddToVector(Lib.CombineVector(_currentChunkCoordinate, entity.Position.ToVector3()), 0, 0.5f, 0);
                    break;
            }
        }

        void InstantiatePrefab(EntityData entity)
        {
            _currentInstance = Instantiate(Resources.Load<GameObject>($"prefab/{entity.ID}"));
            entityDataHandler = _currentInstance.AddComponent<EntityDataHandler>();
            entityDataHandler._entityData = entity;
            _currentInstance.transform.parent = transform;
        }
    } 
}

[System.Serializable]
public enum EntityType
{
    Item,
    Static,
    Rigid
}
  