using System;
using TMPro;

[System.Serializable]
public class EnemyInfo : MobInfo
{
    // [NonSerialized] private TextMeshPro _textMeshPro;
    public override void Initialize()
    {
        base.Initialize();
        // _textMeshPro = Machine.transform.Find("text").GetComponent<TextMeshPro>();
        Health = HealthMax;
        HitboxType = HitboxType.Enemy;
        TargetHitboxType = HitboxType.Player; 
        ActionType = IActionType.Hit;
        // Target = Game.PlayerInfo;
    }

    protected override void OnUpdate()
    { 
        base.OnUpdate(); 
        // _textMeshPro.text = Health.ToString();
        FaceTarget = Target != null;
        SpeedTarget = IsGrounded? SpeedGround : SpeedAir; 
        SpeedTarget *= SpeedModifier;
        if (Health <= 0)
        { 
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