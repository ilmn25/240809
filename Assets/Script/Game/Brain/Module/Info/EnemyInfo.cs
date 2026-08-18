using System;

[System.Serializable]
public class EnemyInfo : MobInfo
{
    /// <summary>Raised when an enemy dies, with the killed enemy's id.</summary>
    public static event System.Action<ID> Killed;

    public override void Initialize()
    {
        base.Initialize();
        Health = HealthMax;
        HitboxType = HitboxType.Enemy;
        targetHitboxType = HitboxType.Player; 
        ActionType = IActionType.Hit;
    }

    protected override void OnUpdate()
    { 
        base.OnUpdate(); 
        FaceTarget = Target != null;
        SpeedTarget = IsGrounded? SpeedGround : SpeedAir; 
        SpeedTarget *= SpeedModifier;
        if (Health <= 0)
        { 
            Killed?.Invoke(((EntityMachine)Machine).Info.id);
            Loot.Gettable(((EntityMachine)Machine).Info.id).Spawn(Machine.transform.position);
            Destroy();
            Audio.PlaySFX(DeathSfx);
        }
    }

    protected override void OnHit(Projectile projectile)
    { 
        if (Target == null)
        { 
            Machine.SetState<DefaultState>(); 
        } 
        Target = projectile.SourceInfo;
        PathingStatus = PathingStatus.Reached;  
        Machine.SetState<MobHit>();
    }
 
}