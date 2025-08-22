using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MobSpriteCullModule : SpriteCullModule
{ 
    public override void Initialize()
    {
        Sprites.Add(Info.SpriteCharRenderer);
        Sprites.Add(Info.SpriteToolRenderer);
        MapCull.SignalUpdateSpriteYCull += HandleCull;
        HandleCull(); 
    }
}

public class StructureSpriteCullModule : SpriteCullModule
{
    public override void Initialize()
    {
        Sprites.Add(Machine.transform.Find("Sprite").GetComponent<SpriteRenderer>());
        MapCull.SignalUpdateSpriteYCull += HandleCull;
        HandleCull();
    }
}


public class ItemSpriteCullModule : SpriteCullModule
{
    public override void Initialize()
    {
        Sprites.Add(Machine.GetComponent<SpriteRenderer>());
        MapCull.SignalUpdateSpriteYCull += HandleCull;
        HandleCull();
    }
}
public class SpriteCullModule : MobModule
{ 
    protected List<SpriteRenderer> Sprites = new List<SpriteRenderer>();
     

    public override void Terminate()
    {
        MapCull.SignalUpdateSpriteYCull -= HandleCull;
    }
 
    protected void HandleCull()
    {
        if (MapCull.YCheck && Machine.transform.position.y > MapCull.YThreshold)
        {
            foreach (SpriteRenderer sprite in Sprites )
            {
                sprite.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            } 
        }
        else
        {
            foreach (SpriteRenderer sprite in Sprites )
            {
                sprite.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            } 
        } 
    }
}
