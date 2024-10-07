using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteOrbitHandler : MonoBehaviour
{
    private SpriteRenderer _sprite;
    void Awake()
    {
        _sprite = GetComponent<SpriteRenderer>();
        transform.rotation = CameraSystem._currentRotation;
    }
    
    void Start()
    {   
        CameraSystem.OnOrbitRotate += UpdateOrbit;
    }
    
    void OnDestroy()
    {
        CameraSystem.OnOrbitRotate -= UpdateOrbit; 
    }   
    
    void OnBecameVisible() {
        transform.rotation = CameraSystem._currentRotation;
    }
 
    void UpdateOrbit()
    { 
        if (_sprite.isVisible) transform.rotation = CameraSystem._currentRotation;
    }
}
