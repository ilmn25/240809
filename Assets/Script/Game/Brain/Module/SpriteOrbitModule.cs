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
        if (!_sprite) _sprite = Machine.transform.Find("Sprite");
        _sprite.rotation = ViewPort.CurrentRotation;
        ViewPort.UpdateOrbitRotate += UpdateOrbit;
    }
    
    public override void Terminate()
    {
        ViewPort.UpdateOrbitRotate -= UpdateOrbit; 
    }   
 
    private void UpdateOrbit()
    { 
        if (EntityMachine?.Info is PlayerInfo pi && pi.PlayerStatus == PlayerStatus.Incapacitated) return;
        _sprite.rotation = ViewPort.CurrentRotation;
    }
}
