using UnityEngine;

public class StructureMachine : EntityMachine
{
    protected SpriteRenderer SpriteRenderer;
    public override void OnSetup()
    {
        SpriteRenderer = transform.Find("Sprite").GetComponent<SpriteRenderer>();
        SpriteRenderer.sprite = Cache.LoadSprite("Sprite/" + Info.stringID);
    }
    public override void OnStart()
    { 
        AddModule(new SpriteOrbitModule()); 
        AddModule(new StructureSpriteCullModule());   
    }
} 