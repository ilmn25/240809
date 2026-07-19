using System;
using UnityEngine;

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
        // Track by uid so the server can find and destroy entities (e.g. client pickup relay).
        if (string.IsNullOrEmpty(info.uid))
            info.uid = Guid.NewGuid().ToString("N");
        Info.Dictionary[info.uid] = info;
        if (!_initialSetup)
        { 
            _initialSetup = true;
            Entity = Entity.Dictionary[info.id]; 
            gameObject.layer = Entity.Collision;
            if (Entity.Bounds != Vector3Int.zero)
            {
                BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
                boxCollider.center = new Vector3(0, Entity.Bounds.y / 2, 0); 
                boxCollider.size = Entity.Bounds; 
            } 
            OnSetup();
        }  
        StartInternal();
    } 
      
    public void Unload()
    { 
        Info.Dictionary.Remove(Info.uid);
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
        // Only the owner runs AI/pathfinding/host-only modules
        if (Info != null && Info.IsOwner())
        {
            OnUpdate();
            RunForMode(Module.UpdateMode.OwnerOnly);
        }
        if (Info != null && Info.Destroyed)
        {
            // Broadcast destruction to remote clients so they remove the entity too.
            // Covers all destroy paths: ItemMachine merge stacking, BlockMachine, despawn, etc.
            EntitySync.BroadcastEntityUnload(Info);
            Unload();
        }
    }

    public virtual void Attack() { }
}

