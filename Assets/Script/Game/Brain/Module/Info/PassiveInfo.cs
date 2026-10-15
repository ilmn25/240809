using System;

[System.Serializable]
public class PassiveInfo : MobInfo
{
    /// <summary>The baseline info for a friendly NPC — a trader, the guide, a rescued person:
    /// tough enough to survive a stray hit and roaming a short leash. Callers add their own
    /// sprite or combat reach on top.</summary>
    public static PassiveInfo CreateNpc() => new PassiveInfo()
    {
        HealthMax = 50,
        SpeedGround = 5,
        SpeedAir = 6,
        DistRoam = 3,
        IsNPC = true,
    };

    public override void Initialize()
    {
        base.Initialize();
        Health = HealthMax;
        HitboxType = HitboxType.Passive;
        targetHitboxType = HitboxType.Player;
        ActionType = IActionType.Hit;
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        FaceTarget = Target != null;
        SpeedTarget = IsGrounded ? SpeedGround : SpeedAir;
        SpeedTarget *= SpeedModifier;
        if (Health <= 0)
        {
            if (Machine.IsInteracting)
                Machine.SetState<DefaultState>();
            Loot.Gettable(((EntityMachine)Machine).Info.id).Spawn(Machine.transform.position);
            Destroy();
            Audio.PlaySFX(DeathSfx);
        }
    }

    protected override void OnHit(Projectile projectile)
    {
        if (Machine.IsInteracting) return;

        if (Target == null)
        {
            Machine.SetState<DefaultState>();
        }
        Target = projectile.SourceInfo;
        PathingStatus = PathingStatus.Reached;
        Machine.SetState<MobHit>();
    }
}
