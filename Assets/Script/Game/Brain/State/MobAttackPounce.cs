
using UnityEngine;

public class MobAttackPounce : MobState
{ 
    private readonly ProjectileInfo _projectileInfo = 
        new ContactDamageProjectileInfo(10, 15, 3, 1.1f);

    private int _delay = 0;

    public override void Initialize()
    {
        _delay = 0;
    }

    public override void OnEnterState()
    { 
        Vector3 dir = Info.Target.transform.position - Machine.transform.position;
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
            Projectile.Spawn(Machine.transform.position,Info.Target.transform.position,
                _projectileInfo, HitboxType.Friendly);
        } 
    }
}
