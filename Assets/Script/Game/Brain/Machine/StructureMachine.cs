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