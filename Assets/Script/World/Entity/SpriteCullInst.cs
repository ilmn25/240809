using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteCullInst : MonoBehaviour
{
    private MapCullStatic _mapCullStatic;
    private SpriteRenderer _sprite;

    void Start()
    {
        _sprite = GetComponent<SpriteRenderer>();
        _mapCullStatic = GameObject.Find("map_system").GetComponent<MapCullStatic>();
        MapCullStatic._signalUpdateSpriteYCull += HandleCull;
        HandleCull(); 
    }

    void OnDestroy() {
        MapCullStatic._signalUpdateSpriteYCull -= HandleCull;
    }
 
    void HandleCull()
    {  
        _sprite.enabled = !(_mapCullStatic._yCheck && transform.position.y > _mapCullStatic._yThreshold); 
    }
}
