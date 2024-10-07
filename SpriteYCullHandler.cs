using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteYCullHandler : MonoBehaviour
{
    private MapYCullSystem _mapYCullSystem;
    private SpriteRenderer _sprite;

    void Start()
    {
        _sprite = GetComponent<SpriteRenderer>();
        _mapYCullSystem = GameObject.Find("map_system").GetComponent<MapYCullSystem>();
        MapYCullSystem._signalUpdateSpriteYCull += HandleCull;
        HandleCull(); 
    }

    void OnDestroy() {
        MapYCullSystem._signalUpdateSpriteYCull -= HandleCull;
    }
 
    void HandleCull()
    {  
        _sprite.enabled = !(_mapYCullSystem._yCheck && transform.position.y > _mapYCullSystem._yThreshold); 
    }
}
