using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class EntityMachine : Machine
{ 
    private ChunkEntityData _entityData;
    protected override void Awake() { }
    
    public ChunkEntityData GetEntityData()
    {
        _entityData.position = new SerializableVector3Int(WorldSingleton.GetBlockCoordinate(transform.position));
        return _entityData;
    }
 
    public void Initialize(ChunkEntityData entityData) { 
        _entityData = entityData; 
        InitializeInteral();
    }

    public void WipeEntity()
    {
        if (EntitySingleton.dictionary[_entityData.stringID].Type == EntityType.Static)
            EntityStaticLoadSingleton._activeEntities[WorldSingleton.GetChunkCoordinate(transform.position)].Item2
                .Remove(this);
        else
            EntityDynamicLoadSingleton.ForgetEntity(this);
        
        EntityPoolSingleton.Instance.ReturnObject(gameObject); 
    }  
}

