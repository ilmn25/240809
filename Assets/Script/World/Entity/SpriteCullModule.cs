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
        _sprite.enabled = !(MapCullSingleton.Instance._yCheck && transform.position.y > MapCullSingleton.Instance._yThreshold); 
    }
}
