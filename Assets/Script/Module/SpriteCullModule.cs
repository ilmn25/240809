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
        MapCullSingleton._signalUpdateSpriteYCull += HandleCull;
        HandleCull(); 
    }

    public override void Terminate()
    {
        MapCullSingleton._signalUpdateSpriteYCull -= HandleCull;
    }
 
    void HandleCull()
    {
        if (MapCullSingleton.Instance._yCheck && Machine.transform.position.y > MapCullSingleton.Instance._yThreshold)
        {
            _sprite.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        }
        else
        {
            _sprite.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        } 
    }
}
