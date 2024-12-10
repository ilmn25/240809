using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteCullModule : MonoBehaviour
{
    private SpriteRenderer _sprite;

    void Start()
    {
        _sprite = GetComponent<SpriteRenderer>();
        MapCullSingleton._signalUpdateSpriteYCull += HandleCull;
        HandleCull(); 
    }

    void OnDestroy() {
        MapCullSingleton._signalUpdateSpriteYCull -= HandleCull;
    }
 
    void HandleCull()
    {
        if (MapCullSingleton.Instance._yCheck && transform.position.y > MapCullSingleton.Instance._yThreshold)
        {
            _sprite.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        }
        else
        {
            _sprite.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        } 
    }
}
