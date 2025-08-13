
using UnityEngine;

public class MobAttackPounce : MobState
{ 
    private readonly ProjectileInfo _projectileInfo = new ContactDamageProjectileInfo {
            Damage = 1,
            Knockback = 10,
            CritChance = 0.1f,
            Radius = 0.7f,
        };

    private int _delay = 0;

    public override void Initialize()
    {
        _delay = 0;
    }

    public override void OnEnterState()
    { 
        Vector3 dir = Info.AimPosition - Machine.transform.position;
        dir.y = 0;
        dir = dir.normalized;
        dir.y = 0.5f;
        Info.Direction = dir; 
    }

    public override void OnUpdateState()    
    {
        _delay++;
        if (_delay > 370)
        {
            Machine.SetState<DefaultState>();
            _delay = 0;
        }
        else if (_delay > 30)
        {
            Projectile.Spawn(Info.SpriteToolTrack.position,Info.AimPosition,
                _projectileInfo, Info.TargetHitboxType, Machine);
        } 
    }
}
