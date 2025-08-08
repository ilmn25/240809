using System;
using UnityEngine;

public class StructureMachine : EntityMachine
{
    protected SpriteRenderer SpriteRenderer;
    public override void OnSetup()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
        SpriteRenderer.sprite = Cache.LoadSprite("sprite/" + entityData.stringID);
    }
}

public class DestructableMachine : StructureMachine, IHitBox
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
        AddModule(new SpriteCullModule(SpriteRenderer));  
        AddModule(new SpriteOrbitModule(SpriteRenderer)); 
        AddModule(new StaticInfo(_health, entityData.stringID, _sfx)); 
    }
 
}