using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class EntityMachine : Machine, IInfoProvider
{ 
    public Info Info => GetModule<Info>();
    public Entity Entity;
    private bool _initialSetup = false;   

    public static Info NewInfo() => new Info();
    public virtual void OnSetup() {}
    
    public void Initialize(Info info) { 
        Modules.Clear();
        States.Clear();
        StateCurrent = State.DefaultState;
        StatePrevious = State.DefaultState;
        AddModule(info); 
        if (!_initialSetup)
        { 
            _initialSetup = true;
            Entity = Entity.Dictionary[info.id]; 
            gameObject.layer = Entity.Collision;
            if (Entity.Bounds != Vector3Int.zero)
            {
                BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
                boxCollider.center = new Vector3(0, (float)Entity.Bounds.y / 2, 0); 
                boxCollider.size = Entity.Bounds; 
            } 
            OnSetup();
        }  
        StartInternal();
    } 
      
    public void Unload()
    { 
        Info.Machine = null;
        if (Entity.StaticLoad)
        {
            Info.IsInRenderRange = false;
            EntityStaticLoad.ForgetEntity(this, Entity);
        } 
        else 
            EntityDynamicLoad.ForgetEntity(this);
        ObjectPool.ReturnObject(gameObject); 
    }

    public override void Update()
    {
        base.Update();
        if (Info.Destroyed) Unload();
    }

    public virtual void Attack() { }
}

