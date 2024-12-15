using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteOrbitModule : MonoBehaviour
{
    private SpriteRenderer _sprite;
    void Awake()
    {
        _sprite = GetComponent<SpriteRenderer>();
        transform.rotation = CameraSingleton._currentRotation;
    }
    
    void Start()
    {   
        CameraSingleton.UpdateOrbitRotate += UpdateOrbit;
    }
    
    void OnDestroy()
    {
        CameraSingleton.UpdateOrbitRotate -= UpdateOrbit; 
    }   
    
    void OnBecameVisible() {
        transform.rotation = CameraSingleton._currentRotation;
    }
 
    void UpdateOrbit()
    { 
        if (_sprite.isVisible) transform.rotation = CameraSingleton._currentRotation;
    }
}
