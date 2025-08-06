
using UnityEngine;

public class GhoulPounce : State
{
    private MobStatusModule _mobStatusModule;
    private readonly ProjectileInfo _projectileInfo = 
        new SwingProjectileInfo(10, 18, 3, 1.1f, 0.5f);

    private int _delay = 0;
    public override void OnInitialize()
    {
        _mobStatusModule = Machine.GetModule<MobStatusModule>();
    }

    public override void OnEnterState()
    {
        Projectile.Spawn(Machine.transform.position,_mobStatusModule.Target.transform.position,
            _projectileInfo, HitboxType.Friendly);
        Vector3 dir = _mobStatusModule.Target.transform.position - Machine.transform.position;
        dir.y = 0;
        dir = dir.normalized;
        dir.y = 0.5f;
        _mobStatusModule.Direction = dir; 
    }

    public override void OnUpdateState()    
    {
        _delay++;
        if (_delay > 55)
        {
            Parent.SetState<StateEmpty>();
            _delay = 0;
        }
    }
}
