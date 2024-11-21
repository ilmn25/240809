using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteCullInst : MonoBehaviour
{
    private SpriteRenderer _sprite;

    void Start()
    {
        _sprite = GetComponent<SpriteRenderer>();
        MapCullStatic._signalUpdateSpriteYCull += HandleCull;
        HandleCull(); 
    }

    void OnDestroy() {
        MapCullStatic._signalUpdateSpriteYCull -= HandleCull;
    }
 
    void HandleCull()
    {  
        _sprite.enabled = !(MapCullStatic.Instance._yCheck && transform.position.y > MapCullStatic.Instance._yThreshold); 
    }
}
