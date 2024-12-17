using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteCullModule : Module
{
    private SpriteRenderer _sprite;

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
 
    void HandleCull()
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
