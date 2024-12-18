using System;
using UnityEngine;

public class RockMachine : EntityMachine, IActionPrimary
{
    public event Action Hit;
    public override void OnInitialize()
    {
        AddState(new RockState());
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        AddModule(new SpriteCullModule(spriteRenderer)); 
        AddModule(new SpriteOrbitModule(spriteRenderer)); 
    }

    public void OnActionPrimary()
    {
        Hit?.Invoke();
    }
}