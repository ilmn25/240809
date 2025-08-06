using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MobSpriteOrbitModule : SpriteOrbitModule
{
    public override void Initialize()
    {
        _sprite = Machine.transform.Find("sprite").Find("char").GetComponent<SpriteRenderer>();
        Machine.transform.rotation = ViewPort.CurrentRotation;
        ViewPort.UpdateOrbitRotate += UpdateOrbit;
    }
}
public class SpriteOrbitModule : Module
{
    protected SpriteRenderer _sprite;

    public SpriteOrbitModule(SpriteRenderer sprite = null)
    {
        _sprite = sprite;
    }
    
    public override void Initialize()
    {
        if (!_sprite) _sprite = Machine.transform.Find("sprite").GetComponent<SpriteRenderer>();
        Machine.transform.rotation = ViewPort.CurrentRotation;
        ViewPort.UpdateOrbitRotate += UpdateOrbit;
    }
    
    public override void Terminate()
    {
        ViewPort.UpdateOrbitRotate -= UpdateOrbit; 
    }   
 
    protected void UpdateOrbit()
    { 
        Machine.transform.rotation = ViewPort.CurrentRotation;
    }
}
