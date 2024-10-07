using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class EntityLoadSystem : MonoBehaviour
{
    private ChunkSystem _chunkSystem;
    private ItemLoadSystem _itemLoadSystem;
    public static event Action UpdateEntityParent; 
    int _chunkSize;

    public static Dictionary<Vector3Int, (List<Entity>, List<EntityHandler>)> _entityList = new Dictionary<Vector3Int, (List<Entity>, List<EntityHandler>)>();
    public int ENTITY_DISTANCE = 2;

    void Awake()
    {
        _chunkSystem = GameObject.Find("world_system").GetComponent<ChunkSystem>();
        _itemLoadSystem = GetComponent<ItemLoadSystem>();
        ChunkSystem.PlayerChunkPositionUpdate += HandleChunkEntityTraverse; 
        _chunkSize = ChunkSystem.CHUNKSIZE;
    }

    async void Start()
    {
        await Task.Delay(7000);
        HandleChunkEntityTraverse();
    }

    public void SaveAllEntities()
    {
        UpdateEntityParent?.Invoke();
        foreach (var key in _entityList.Keys)
        {
            // Clear the entity list for the current key
            _entityList[key].Item1.Clear();

            // Loop through each EntityHandler in the second list and add the updated entity to the first list
            foreach (var entityHandler in _entityList[key].Item2)
            {
                _entityList[key].Item1.Add(entityHandler.GetUpdatedEntity());
            }
        }
    }

    private void HandleChunkEntityTraverse()
    {
        // Stopwatch stopwatch = Stopwatch.StartNew();
        Vector3 currentChunkPosition = ChunkSystem._chunkPosition; 
        HandleUnload();
        HandleLoad(); 
        // stopwatch.Stop();
        // CustomLibrary.Log("Elapsed time: " + stopwatch.ElapsedMilliseconds + " ms");
        void HandleUnload()
        {
            var keysToRemove = new List<Vector3Int>();
            UpdateEntityParent?.Invoke();

            foreach (var key in _entityList.Keys)
            {
                // Extract chunk coordinates from the key
                int chunkX = key.x;
                int chunkZ = key.z;

                if (chunkX > currentChunkPosition.x + ENTITY_DISTANCE * _chunkSize || chunkX < currentChunkPosition.x - ENTITY_DISTANCE * _chunkSize
                    || chunkZ > currentChunkPosition.z + ENTITY_DISTANCE * _chunkSize || chunkZ < currentChunkPosition.z - ENTITY_DISTANCE * _chunkSize)
                {
                    // Clear the entity list for the current key
                    _entityList[key].Item1.Clear();

                    // Loop through each EntityHandler in the second list and add the updated entity to the first list
                    foreach (EntityHandler entityHandler in _entityList[key].Item2)
                    { 
                        _entityList[key].Item1.Add(entityHandler.GetUpdatedEntity());

                        // Destroy the parent GameObject of the EntityHandler
                        Destroy(entityHandler.gameObject);  
                    }
                    keysToRemove.Add(key);
                }
            }
            // Remove the marked keys from the dictionary
            foreach (var key in keysToRemove)
            { 
                _entityList.Remove(key);
            }
        }

 

        void HandleLoad()
        {
            _chunkSystem.HandleLoadRegion(0); 

            for (int x = -ENTITY_DISTANCE * _chunkSize; x <= ENTITY_DISTANCE * _chunkSize; x += _chunkSize)
            {
                for (int z = -ENTITY_DISTANCE * _chunkSize; z <= ENTITY_DISTANCE * _chunkSize; z += _chunkSize)
                {
                    chunkCoordinates = new Vector3Int(
                        Mathf.FloorToInt(currentChunkPosition.x + x),
                        0,
                        Mathf.FloorToInt(currentChunkPosition.z + z)
                    );

                    if (!_entityList.ContainsKey(chunkCoordinates))
                    {
                        _currentChunk = _chunkSystem.LoadChunk(chunkCoordinates); 
                        if  (_currentChunk != null)
                        {
                            _entityChunk = _currentChunk.Entity;  
                            LoadChunkEntities(); 
                        }
                    }  
                } 
            } 
        }
    }
     
    Vector3Int chunkCoordinates;
    List<Entity> _entityChunk;
    Chunk _currentChunk; 
    
    public void LoadChunkEntities()
    {
        GameObject instance;
        EntityHandler entityHandler;

        // Find the key once
        if (!_entityList.ContainsKey(chunkCoordinates))
        {
            _entityList[chunkCoordinates] = (_entityChunk, new List<EntityHandler>());
        }

        foreach (Entity entity in _entityChunk)
        { 
            switch (entity.Type)
            {
                case EntityType.Item:
                    _itemLoadSystem.SpawnItem(int.Parse(entity.ID),
                    CustomLibrary.CombineVector(chunkCoordinates, entity.Position.ToVector3()));
                    break;

                case EntityType.Static:
                    InstantiatePrefab(entity);
                    instance.transform.position = CustomLibrary.CombineVector(chunkCoordinates, entity.Position.ToVector3());
                    break;

                case EntityType.Rigid:
                    InstantiatePrefab(entity);
                    instance.transform.position = CustomLibrary.AddToVector(CustomLibrary.CombineVector(chunkCoordinates, entity.Position.ToVector3()), 0, 0.5f, 0);
                    // instance.transform.name = instance.transform.position.ToString();
                    break;
            }
        }

        void InstantiatePrefab(Entity entity)
        {
            instance = Instantiate(Resources.Load<GameObject>($"prefab/{entity.ID}"));
            entityHandler = instance.AddComponent<EntityHandler>();
            entityHandler._entity = entity;
            instance.AddComponent<EntityChunkPositionHandler>();
            instance.transform.parent = transform;
            // Add the EntityHandler to the local handler list
            // _handlerList.Add(entityHandler);
        }
    } 
}

[System.Serializable]
public class Entity
{
    public string ID { get; set; }
    public Dictionary<string, object> Parameters { get; set; }
    public SerializableVector3 Position { get; set; }
    public SerializableVector3Int Bounds { get; set; }
    public EntityType Type { get; set; }

    public Entity(string id, SerializableVector3 position, SerializableVector3Int bounds = null, Dictionary<string, object> parameters = null, EntityType type = EntityType.Static)
    {
        ID = id;
        Position = position;
        Bounds = bounds ?? new SerializableVector3Int(0, 0, 0);
        Parameters = parameters;
        Type = type;
    }
}

[System.Serializable]
public enum EntityType
{
    Item,
    Static,
    Rigid
}
  