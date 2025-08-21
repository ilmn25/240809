
using UnityEngine;

public class MobAttackPounce : MobState
{ 
    private readonly ProjectileInfo _projectileInfo = new ContactDamageProjectileInfo {
            Damage = 1,
            Knockback = 10,
            CritChance = 0.1f,
            Radius = 0.3f,
        };

    private int _delay = 0;
    private bool _jumpCheck = false;
    private int _pounceCount = 0;
    public int Pounces = 5;

    public MobAttackPounce(int pounces)
    {
        Pounces = pounces;
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
        if (_delay > 5)
        {
            Projectile.Spawn(Info.SpriteToolTrack.position,Info.AimPosition,
                _projectileInfo, Info.TargetHitboxType, Info);
        }

        if (Info.IsGrounded)
        {
            if (!_jumpCheck)
            {
                _jumpCheck = true;
                _pounceCount++;
                if (_pounceCount == Pounces)
                {
                    _pounceCount = 0;
                    _delay = 0;
                    Machine.SetState<DefaultState>();
                }
            } 
        }
        else _jumpCheck = false;
    }
}
