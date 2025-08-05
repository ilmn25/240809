using System;
using UnityEngine;

public class RockMachine : EntityMachine, IHitBox
{
    public event Action Hit;
    public override void OnInitialize()
    {
        // AddState(new RockState());
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        AddModule(new SpriteCullModule(spriteRenderer));  
        AddModule(new SpriteOrbitModule(spriteRenderer)); 
        AddModule(new DestructableModule(60, "brick", "dig_metal")); 
    } 
}