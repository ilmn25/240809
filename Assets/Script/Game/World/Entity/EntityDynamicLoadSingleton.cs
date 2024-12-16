using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class EntityDynamicLoadSingleton : MonoBehaviour
{
    public static EntityDynamicLoadSingleton Instance { get; private set; }    

    private static List<EntityMachine> _activeEntities = new List<EntityMachine>(); 
    
    void Awake()
    {
        Instance = this;
        WorldSingleton.PlayerChunkTraverse += ScanAndUnload; 
        WorldSingleton.PlayerChunkTraverse += ScanAndLoad; 
    }

    public static void ForgetEntity(EntityMachine entity) { _activeEntities.Remove(entity); }
    public static void InviteEntity(EntityMachine entity) { _activeEntities.Add(entity); }
    
    void ScanAndUnload()
    {
        List<EntityMachine> removeList = new List<EntityMachine>();
        Vector3Int entityChunkPosition;
        foreach (var entityMachine in _activeEntities)
        { 
            entityChunkPosition = WorldSingleton.GetChunkCoordinate(entityMachine.transform.position);
            
            if (!WorldSingleton.InPlayerRange(entityChunkPosition, WorldSingleton.LOGIC_DISTANCE))
            {
                if (WorldSingleton.Instance.IsInWorldBounds(entityChunkPosition))
                    WorldSingleton.Instance.GetChunk(entityChunkPosition).DynamicEntity.Add(entityMachine.GetEntityData());
                removeList.Add(entityMachine);
                EntityPoolSingleton.Instance.ReturnObject(entityMachine.gameObject); 
            }
        }
        foreach (var entityMachine in removeList) _activeEntities.Remove(entityMachine);
    }
 
    
    void ScanAndLoad()
    {
        // Collect chunk coordinates within render distance
        for (int x = -WorldSingleton.LOGIC_RANGE; x <= WorldSingleton.LOGIC_RANGE; x++)
        {
            for (int y = -WorldSingleton.LOGIC_RANGE; y <= WorldSingleton.LOGIC_RANGE; y++)
            {
                for (int z = -WorldSingleton.LOGIC_RANGE; z <= WorldSingleton.LOGIC_RANGE; z++)
                {
                    LoadEntitiesInChunk(new Vector3Int(
                        WorldSingleton.PlayerChunkPosition.x + x * WorldSingleton.CHUNK_SIZE,
                        WorldSingleton.PlayerChunkPosition.y + y * WorldSingleton.CHUNK_SIZE,
                        WorldSingleton.PlayerChunkPosition.z + z * WorldSingleton.CHUNK_SIZE
                    ));
                }
            }
        } 
    } 
      
    public void UnloadWorld()
    {
        Vector3Int entityChunkPosition;
        foreach (EntityMachine entityHandler in _activeEntities)
        {
            entityChunkPosition = WorldSingleton.GetChunkCoordinate(entityHandler.transform.position);
            if (WorldSingleton.Instance.IsInWorldBounds(entityChunkPosition))
                WorldSingleton.Instance.GetChunk(entityChunkPosition).DynamicEntity.Add(entityHandler.GetEntityData());
            EntityPoolSingleton.Instance.ReturnObject(entityHandler.gameObject);   
        }
        _activeEntities.Clear();
    }

    public void LoadEntitiesInChunk(Vector3Int chunkCoordinate)
    { 
        EntityMachine currentEntityMachine;
        GameObject currentInstance = null;
        List<ChunkEntityData> chunkEntityList = WorldSingleton.World[chunkCoordinate].DynamicEntity;
        foreach (ChunkEntityData entityData in chunkEntityList)
        {   
            switch (EntitySingleton.dictionary[entityData.stringID].Type)
            {
                case EntityType.Item: 
                    currentInstance = EntityPoolSingleton.Instance.GetObject("item"); 
                    currentInstance.transform.position = chunkCoordinate + entityData.position.ToVector3Int() + new Vector3(0.5f, 0.5f, 0.5f); 
        
                    currentInstance.GetComponent<SpriteRenderer>().sprite = 
                        Resources.Load<Sprite>($"texture/sprite/{entityData.stringID}"); 
                    break;

                case EntityType.Rigid:
                    // Lib.Log(((NPCCED)entityData).npcStatus);
                    currentInstance = EntityPoolSingleton.Instance.GetObject(entityData.stringID);
                    currentInstance.transform.position = chunkCoordinate + entityData.position.ToVector3Int() + new Vector3(0.5f, 0.5f, 0.5f); 
                    break;
            }
            
            currentEntityMachine = currentInstance.GetComponent<EntityMachine>();
            _activeEntities.Add(currentEntityMachine); 
            currentEntityMachine.Initialize(entityData);
        }
        chunkEntityList.Clear();
    } 
}
 