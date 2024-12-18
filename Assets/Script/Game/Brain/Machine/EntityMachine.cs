using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class EntityMachine : Machine
{ 
    public ChunkEntityData entityData;
    private bool _awake;
    protected override void Awake() {  } 
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
        if (!_awake)
        {
            _awake = true;
            IEntityData entity = Entity.dictionary[entityData.stringID];
            if (entity.Type == EntityType.Static)
            {
                gameObject.layer = Game.IndexStatic;
                gameObject.isStatic = true; 
                BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
                boxCollider.size = entity.Bounds;
                boxCollider.center = new Vector3(0, entity.Bounds.y / 2, 0);
            }
            else
            {
                gameObject.layer = Game.IndexDynamic;
                gameObject.AddComponent<SphereCollider>();
            }  
        } 
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

