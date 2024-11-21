using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class EntityLoadStatic : MonoBehaviour
{
    private WorldStatic _worldStatic;
    private ItemLoadStatic _itemLoadStatic;
    public static event Action UpdateEntityParent; 
    int _chunkSize;

    public static Dictionary<Vector3Int, (List<EntityData>, List<EntityAbstract>)> _entityList = new Dictionary<Vector3Int, (List<EntityData>, List<EntityAbstract>)>();
    public int ENTITY_DISTANCE = 2;

    void Awake()
    {
        _worldStatic = GameObject.Find("world_system").GetComponent<WorldStatic>();
        _itemLoadStatic = GetComponent<ItemLoadStatic>();
        WorldStatic.PlayerChunkPositionUpdate += HandleChunkEntityTraverse; 
        _chunkSize = WorldStatic.CHUNKSIZE;
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
        Vector3 currentChunkPosition = WorldStatic._chunkPosition; 
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
                    foreach (EntityAbstract entityHandler in _entityList[key].Item2)
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
            _worldStatic.HandleLoadWorldFile(0); 

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
                        _currentChunkData = _worldStatic.GetChunk(chunkCoordinates); 
                        if  (_currentChunkData != null)
                        {
                            _entityChunk = _currentChunkData.Entity;  
                            LoadChunkEntities(); 
                        }
                    }  
                } 
            } 
        }
    }
     
    Vector3Int chunkCoordinates;
    List<EntityData> _entityChunk;
    ChunkData _currentChunkData; 
    
    public void LoadChunkEntities()
    {
        GameObject instance;
        EntityAbstract entityAbstract;

        // Find the key once
        if (!_entityList.ContainsKey(chunkCoordinates))
        {
            _entityList[chunkCoordinates] = (_entityChunk, new List<EntityAbstract>());
        }

        foreach (EntityData entity in _entityChunk)
        { 
            switch (entity.Type)
            {
                case EntityType.Item:
                    entity.Position = new SerializableVector3(Lib.CombineVector(chunkCoordinates, entity.Position.ToVector3()));
                    _itemLoadStatic.SpawnItem(entity);
                    break;

                case EntityType.Static:
                    InstantiatePrefab(entity);
                    instance.transform.position = Lib.CombineVector(chunkCoordinates, entity.Position.ToVector3());
                    break;

                case EntityType.Rigid:
                    InstantiatePrefab(entity);
                    instance.transform.position = Lib.AddToVector(Lib.CombineVector(chunkCoordinates, entity.Position.ToVector3()), 0, 0.5f, 0);
                    // instance.transform.name = instance.transform.position.ToString();
                    break;
            }
        }

        void InstantiatePrefab(EntityData entity)
        {
            instance = Instantiate(Resources.Load<GameObject>($"prefab/{entity.ID}"));
            entityAbstract = instance.AddComponent<EntityAbstract>();
            entityAbstract._entityData = entity;
            instance.AddComponent<EntityChunkPositionInst>();
            instance.transform.parent = transform;
            // Add the EntityHandler to the local handler list
            // _handlerList.Add(entityHandler);
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
  