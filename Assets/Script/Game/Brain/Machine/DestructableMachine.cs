using System;
using UnityEngine;

public class StructureMachine : EntityMachine
{
    protected SpriteRenderer SpriteRenderer;
    public override void OnSetup()
    {
        SpriteRenderer = transform.Find("sprite").GetComponent<SpriteRenderer>();
        SpriteRenderer.sprite = Cache.LoadSprite("sprite/" + Info.stringID);
    }
}

public class DestructableMachine : StructureMachine, IActionPrimaryResource
{
    public override void OnStart()
    { 
        AddModule(new SpriteOrbitModule()); 
        AddModule(new StructureSpriteCullModule());   
    }
 
}