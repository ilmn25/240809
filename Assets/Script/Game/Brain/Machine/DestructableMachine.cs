using System;
using UnityEngine;

public class StructureMachine : EntityMachine
{
    protected SpriteRenderer SpriteRenderer;
    public override void OnSetup()
    {
        SpriteRenderer = transform.Find("sprite").GetComponent<SpriteRenderer>();
        SpriteRenderer.sprite = Cache.LoadSprite("sprite/" + entityData.stringID);
    }
}

public class DestructableMachine : StructureMachine, IHitBoxResource
{
    private String _sfx;
    private int _health; 
    public DestructableMachine(string sfx, int health)
    {
        _sfx = sfx;
        _health = health;
    } 

    public override void OnStart()
    { 
        AddModule(new StructureSpriteCullModule());  
        AddModule(new SpriteOrbitModule()); 
        AddModule(new StructureInfo {
            Health = _health,
            Loot = entityData.stringID,
            SfxHit = _sfx,
            SfxDestroy = _sfx,
        }); 
    }
 
}