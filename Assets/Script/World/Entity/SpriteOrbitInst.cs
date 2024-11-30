using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteOrbitInst : MonoBehaviour
{
    private SpriteRenderer _sprite;
    void Awake()
    {
        _sprite = GetComponent<SpriteRenderer>();
        transform.rotation = CameraSingleton._currentRotation;
    }
    
    void Start()
    {   
        CameraSingleton.OnOrbitRotate += UpdateOrbit;
    }
    
    void OnDestroy()
    {
        CameraSingleton.OnOrbitRotate -= UpdateOrbit; 
    }   
    
    void OnBecameVisible() {
        transform.rotation = CameraSingleton._currentRotation;
    }
 
    void UpdateOrbit()
    { 
        if (_sprite.isVisible) transform.rotation = CameraSingleton._currentRotation;
    }
}
