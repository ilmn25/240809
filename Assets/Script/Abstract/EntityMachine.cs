using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class EntityMachine : StateMachine
{ 
    public ChunkEntityData _entityData; 
    public ChunkEntityData GetEntityData()
    {
        _entityData.position = new SerializableVector3Int(WorldSingleton.GetBlockCoordinate(transform.position));
        return _entityData;
    }
 
    public void Initialize(ChunkEntityData entityData) { 
        _entityData = entityData;  
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

