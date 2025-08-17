using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class EntityMachine : Machine, IInfoProvider
{ 
    public Info Info => GetModule<Info>();
    public Entity Entity;
    private bool _initialSetup;  
    public Info GetEntityData()
    {
        Info.position = Vector3Int.FloorToInt(transform.position);
        UpdateEntityData();
        return Info;
    }

    public static Info NewInfo() => new Info();
    public virtual void OnSetup() {}
    public virtual void UpdateEntityData() { }
    
    public void Initialize(Info data) { 
        Modules.Clear();
        States.Clear();
        StateCurrent = State.DefaultState;
        StatePrevious = State.DefaultState;
        AddModule(data);
        if (!_initialSetup)
        { 
            _initialSetup = true;
            Entity = Entity.Dictionary[data.stringID]; 
            gameObject.layer = Entity.Collision;
            if (Entity.Bounds != Vector3Int.zero)
            {
                BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
                if (data is BlockInfo) 
                    boxCollider.center = new Vector3(0.5f, (float)Entity.Bounds.y / 2, 0.5f);
                else
                    boxCollider.center = new Vector3(0, (float)Entity.Bounds.y / 2, 0); 
                boxCollider.size = Entity.Bounds; 
            } 
            OnSetup();
        }  
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
    
    public virtual void Attack() { }
    
}

