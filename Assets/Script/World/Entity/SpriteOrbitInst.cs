using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteOrbitInst : MonoBehaviour
{
    private SpriteRenderer _sprite;
    void Awake()
    {
        _sprite = GetComponent<SpriteRenderer>();
        transform.rotation = CameraStatic._currentRotation;
    }
    
    void Start()
    {   
        CameraStatic.OnOrbitRotate += UpdateOrbit;
    }
    
    void OnDestroy()
    {
        CameraStatic.OnOrbitRotate -= UpdateOrbit; 
    }   
    
    void OnBecameVisible() {
        transform.rotation = CameraStatic._currentRotation;
    }
 
    void UpdateOrbit()
    { 
        if (_sprite.isVisible) transform.rotation = CameraStatic._currentRotation;
    }
}
