using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class EntityMachine : Machine
{ 
    public ChunkEntityData entityData;
    protected override void Awake() { }
    
    public ChunkEntityData GetEntityData()
    {
        entityData.position = new SerializableVector3Int(WorldSingleton.GetBlockCoordinate(transform.position));
        UpdateEntityData();
        return entityData;
    }
    
    public virtual void UpdateEntityData() { }
    
    public void Initialize(ChunkEntityData entityData) { 
        this.entityData = entityData; 
        InitializeInteral();
    }

    public void WipeEntity()
    {
        if (EntitySingleton.dictionary[entityData.stringID].Type == EntityType.Static)
            EntityStaticLoadSingleton._activeEntities[WorldSingleton.GetChunkCoordinate(transform.position)].Item2
                .Remove(this);
        else
            EntityDynamicLoadSingleton.ForgetEntity(this);
        
        EntityPoolSingleton.Instance.ReturnObject(gameObject); 
    }  
}

