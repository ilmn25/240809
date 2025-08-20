using UnityEngine;

public class SpriteOrbitModule : EntityModule
{
    private Transform _sprite;

    public SpriteOrbitModule(Transform sprite = null)
    {
        _sprite = sprite;
    }

    public override void Initialize()
    { 
        if (!_sprite) _sprite = Machine.transform.Find("sprite");
        _sprite.rotation = ViewPort.CurrentRotation;
        ViewPort.UpdateOrbitRotate += UpdateOrbit;
    }
    
    public override void Terminate()
    {
        ViewPort.UpdateOrbitRotate -= UpdateOrbit; 
    }   
 
    private void UpdateOrbit()
    { 
        _sprite.rotation = ViewPort.CurrentRotation;
    }
}
