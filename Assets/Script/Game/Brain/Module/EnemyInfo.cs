[System.Serializable]
public class EnemyInfo : MobInfo
{
    public override void Initialize()
    {
        base.Initialize();
        Health = HealthMax;
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        FaceTarget = Target;
        SpeedTarget = IsGrounded? SpeedGround : SpeedAir; 
        if (Health <= 0)
        { 
            Loot.Gettable(((EntityMachine)Machine).Info.stringID).Spawn(Machine.transform.position);
            ((EntityMachine)Machine).Delete();
            Audio.PlaySFX(DeathSfx, 0.8f);
        }
    }

    protected override void OnHit(Projectile projectile)
    { 
        if (Target == projectile.Source.transform) return;
        Target = projectile.Source.transform;
        PathingStatus = PathingStatus.Reached; 
        Machine.SetState<DefaultState>();
    }
 
}