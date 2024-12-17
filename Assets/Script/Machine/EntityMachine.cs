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
        entityData.position = new SerializableVector3Int(World.GetBlockCoordinate(transform.position));
        UpdateEntityData();
        return entityData;
    }
    
    public virtual void UpdateEntityData() { }
    
    public void Initialize(ChunkEntityData entityData) { 
        this.entityData = entityData; 
        InitializeInteral();
    }

    public void Delete()
    {
        if (Entity.dictionary[entityData.stringID].Type == EntityType.Static)
            EntityStaticLoad.ForgetEntity(this);
        else
            EntityDynamicLoad.ForgetEntity(this);
        
        ObjectPool.ReturnObject(gameObject); 
    }  
}

