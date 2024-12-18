using System;
using UnityEngine;

public class OakMachine : EntityMachine, IActionPrimary
{
    public event Action Hit;
    public override void OnInitialize()
    {
        AddState(new TreeState("wood", 5));
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        AddModule(new SpriteCullModule(spriteRenderer)); 
        AddModule(new SpriteOrbitModule(spriteRenderer)); 
    }

    public void OnActionPrimary()
    {
        Hit?.Invoke();
    }
}