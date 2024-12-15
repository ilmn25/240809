using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteOrbitModule : Module
{
    private SpriteRenderer _sprite;

    public SpriteOrbitModule(SpriteRenderer sprite = null)
    {
        _sprite = sprite;
    }
    
    public override void Initialize()
    {
        if (!_sprite) _sprite = Machine.transform.Find("sprite").GetComponent<SpriteRenderer>();
        Machine.transform.rotation = CameraSingleton._currentRotation;
        CameraSingleton.UpdateOrbitRotate += UpdateOrbit;
    }
    
    public override void Terminate()
    {
        CameraSingleton.UpdateOrbitRotate -= UpdateOrbit; 
    }   
 
    void UpdateOrbit()
    { 
        if (_sprite.isVisible) Machine.transform.rotation = CameraSingleton._currentRotation;
    }
}
