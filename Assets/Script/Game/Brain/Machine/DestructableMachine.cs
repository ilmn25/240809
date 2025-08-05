using System;
using UnityEngine;
public class DestructableMachine : EntityMachine, IHitBox
{
    private String _sfx;
    private int _health;
    public DestructableMachine(string sfx, int health)
    {
        _sfx = sfx;
        _health = health;
    }

    public override void OnInitialize()
    { 
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        AddModule(new SpriteCullModule(spriteRenderer));  
        AddModule(new SpriteOrbitModule(spriteRenderer)); 
        AddModule(new DestructableModule(_health, entityData.stringID, _sfx)); 
    } 
}