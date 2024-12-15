using System;
using UnityEngine;

public class OakMachine : EntityMachine, ILeftClick
{
    public event Action Hit;
    public override void OnInitialize()
    {
        AddState(new TreeState("wood", 5));
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        AddModule(new SpriteCullModule(spriteRenderer)); 
        AddModule(new SpriteOrbitModule(spriteRenderer)); 
    }

    public void OnLeftClick()
    {
        Hit?.Invoke();
    }
}