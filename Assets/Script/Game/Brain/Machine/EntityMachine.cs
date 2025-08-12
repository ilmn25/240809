using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class EntityMachine : Machine
{ 
    public new MobInfo Info => GetModule<MobInfo>();
    public ChunkEntityData entityData;
    public Entity Entity;
    private bool _initialSetup;  
    public ChunkEntityData GetEntityData()
    {
        entityData.position = new SVector3Int(World.GetBlockCoordinate(transform.position));
        UpdateEntityData();
        return entityData;
    } 
    
    public virtual void OnSetup() {}
    public virtual void UpdateEntityData() { }
    
    public void Initialize(ChunkEntityData data) { 
        entityData = data;   
        if (!_initialSetup)
        { 
            _initialSetup = true;
            Entity = Entity.Dictionary[entityData.stringID]; 
            gameObject.layer = Entity.Collision;
            if (Entity.Bounds != Vector3Int.zero)
            {
                BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
                boxCollider.size = Entity.Bounds;
                boxCollider.center = new Vector3(0, Entity.Bounds.y / 2, 0); 
            } 
            OnSetup();
        } 
        Modules.Clear();
        States.Clear();
        StateCurrent = State.DefaultState;
        StatePrevious = State.DefaultState;
        base.Info = null;
        StartInternal();
    }

    public void Delete()
    { 
        if (Entity.StaticLoad)
            EntityStaticLoad.ForgetEntity(this);
        else 
            EntityDynamicLoad.ForgetEntity(this);
        ObjectPool.ReturnObject(gameObject); 
    }  
}

