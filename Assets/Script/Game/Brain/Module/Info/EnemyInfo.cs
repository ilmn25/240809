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
        FaceTarget = Target != null;
        SpeedTarget = IsGrounded? SpeedGround : SpeedAir; 
        if (Health <= 0)
        { 
            Loot.Gettable(((EntityMachine)Machine).Info.stringID).Spawn(Machine.transform.position);
            Destroy();
            Audio.PlaySFX(SfxID.DeathPlayer);
        }
    }

    protected override void OnHit(Projectile projectile)
    { 
        if (projectile.SourceInfo == Game.PlayerInfo) Game.PlayerInfo.CombatCooldown = 10000;
        Target = projectile.SourceInfo;
        PathingStatus = PathingStatus.Reached; 
        Machine.SetState<DefaultState>();
    }
 
}