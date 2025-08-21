using TMPro;

[System.Serializable]
public class EnemyInfo : MobInfo
{
    private TextMeshPro _textMeshPro;
    public override void Initialize()
    {
        base.Initialize();
        _textMeshPro = Machine.transform.Find("text").GetComponent<TextMeshPro>();
        Health = HealthMax;
        HitboxType = HitboxType.Enemy;
        TargetHitboxType = HitboxType.Player;
        Target = Game.PlayerInfo;
        ActionType = IActionType.Hit;
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        _textMeshPro.text = Health.ToString();
        FaceTarget = Target != null;
        SpeedTarget = IsGrounded? SpeedGround : SpeedAir; 
        if (Health <= 0)
        { 
            Loot.Gettable(((EntityMachine)Machine).Info.stringID).Spawn(Machine.transform.position);
            Destroy();
            Audio.PlaySFX(DeathSfx);
        }
    }

    protected override void OnHit(Projectile projectile)
    { 
        if (projectile.SourceInfo == Game.PlayerInfo) Game.PlayerInfo.CombatCooldown = 10000;
        if (Target == null)
        {
            Machine.SetState<DefaultState>(); 
        } 
        Target = projectile.SourceInfo;
        PathingStatus = PathingStatus.Reached;  
    }
 
}