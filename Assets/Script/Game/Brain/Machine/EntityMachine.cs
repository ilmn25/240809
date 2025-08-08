using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class EntityMachine : Machine
{ 
    public new MobInfo Info => GetModule<MobInfo>();
    public ChunkEntityData entityData;
    private bool _initialSetup;  
    public ChunkEntityData GetEntityData()
    {
        entityData.position = new SVector3Int(World.GetBlockCoordinate(transform.position));
        UpdateEntityData();
        return entityData;
    } 
    
    public virtual void UpdateEntityData() { }
    
    public void Initialize(ChunkEntityData data) { 
        entityData = data;   
        
        Modules.Clear();
        States.Clear();
        StateCurrent = State.DefaultState;
        StatePrevious = State.DefaultState;
        base.Info = null;
         
        StartInternal();
        
        if (!_initialSetup)
        {
            _initialSetup = true;
            IEntity entity = Entity.Dictionary[entityData.stringID];
            if (entity.Type == EntityType.Static)
            {
                if (entity.Bounds == Vector3Int.zero)
                {
                    gameObject.layer = Game.IndexDynamic;
                    gameObject.isStatic = false;  
                }
                else
                {
                    gameObject.layer = Game.IndexStatic;
                    gameObject.isStatic = true;  
                } 
                BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
                boxCollider.size = entity.Bounds;
                boxCollider.center = new Vector3(0, entity.Bounds.y / 2, 0);
            }
            else if (entity.Type == EntityType.Rigid)
            {
                gameObject.layer = Game.IndexDynamic;
                gameObject.isStatic = false;  
                gameObject.AddComponent<SphereCollider>();
            }  
        } 
    }

    public void Delete()
    { 
        if (Entity.Dictionary[entityData.stringID].Type == EntityType.Static)
            EntityStaticLoad.ForgetEntity(this);
        else
            EntityDynamicLoad.ForgetEntity(this);  
        ObjectPool.ReturnObject(gameObject); 
    }  
}

