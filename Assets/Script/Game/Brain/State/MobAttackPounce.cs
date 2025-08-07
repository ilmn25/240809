
using UnityEngine;

public class MobAttackPounce : MobAttack
{ 
    private readonly ProjectileInfo _projectileInfo = 
        new ContactDamageProjectileInfo(10, 15, 3, 1.1f);

    private int _delay = 0; 

    public override void OnEnterState()
    { 
        Vector3 dir = Module<MobStatusModule>().Target.transform.position - Machine.transform.position;
        dir.y = 0;
        dir = dir.normalized;
        dir.y = 0.5f;
        Module<MobStatusModule>().Direction = dir; 
    }

    public override void OnUpdateState()    
    {
        _delay++;
        if (_delay > 600 && Module<StatusModule>().IsGrounded)
        {
            Machine.SetState<DefaultState>();
            _delay = 0;
        }
        else if (_delay > 30)
        {
            Projectile.Spawn(Machine.transform.position,Module<MobStatusModule>().Target.transform.position,
                _projectileInfo, HitboxType.Friendly);
        } 
    }
}
