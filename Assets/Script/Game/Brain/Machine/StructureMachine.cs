using UnityEngine;

public class StructureMachine : EntityMachine, IActionPrimaryResource
{
    protected SpriteRenderer SpriteRenderer;
    public override void OnSetup()
    {
        SpriteRenderer = transform.Find("Sprite").GetComponent<SpriteRenderer>();
        SpriteRenderer.sprite = Cache.LoadSprite("Sprite/" + Info.id);
    }
    public override void OnStart()
    { 
        AddModule(new SpriteOrbitModule()); 
        AddModule(new StructureSpriteCullModule());   
    }
} 