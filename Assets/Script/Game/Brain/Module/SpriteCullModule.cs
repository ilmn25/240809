using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MobSpriteCullModule : SpriteCullModule
{ 
    public override void Initialize()
    {
        _sprite = Machine.transform.Find("sprite").Find("char").GetComponent<SpriteRenderer>();
        MapCull.SignalUpdateSpriteYCull += HandleCull;
        HandleCull(); 
    }
}
public class SpriteCullModule : Module
{
    protected SpriteRenderer _sprite;

    public SpriteCullModule(SpriteRenderer sprite = null)
    {
        _sprite = sprite;
    }
    
    public override void Initialize()
    {
        if (!_sprite) _sprite = Machine.transform.Find("sprite").GetComponent<SpriteRenderer>();
        MapCull.SignalUpdateSpriteYCull += HandleCull;
        HandleCull(); 
    }

    public override void Terminate()
    {
        MapCull.SignalUpdateSpriteYCull -= HandleCull;
    }
 
    protected void HandleCull()
    {
        if (MapCull.YCheck && Machine.transform.position.y > MapCull.YThreshold)
        {
            _sprite.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        }
        else
        {
            _sprite.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        } 
    }
}
