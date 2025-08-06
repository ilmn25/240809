using UnityEngine;

public class MobStatusModule : StatusModule
{
    public Transform Target = null;
    public PathingStatus PathingStatus = PathingStatus.Pathing;
    public Vector3 Direction = Vector3.zero;
    public bool IsGrounded = false;

    private readonly string _hurtSfx;
    private readonly string _deathSfx;

    public MobStatusModule(HitboxType hitBoxType, float health, float defense, string hurtSfx, string deathSfx) : base(
        hitBoxType, health, defense)
    {
        _hurtSfx = hurtSfx;
        _deathSfx = deathSfx;
    }

    protected override void OnHit(Projectile projectile)
    {
        Audio.PlaySFX(_hurtSfx, 0.4f);
        Target = Game.Player.transform;
        PathingStatus = PathingStatus.Reached;
        Machine.GetModule<GroundMovementModule>().KnockBack(projectile.transform.position, projectile.Info.Knockback, true);
    }

    protected override void OnDeath()
    {
        Audio.PlaySFX(_deathSfx, 0.8f);
        Loot.Gettable(((EntityMachine)Machine).entityData.stringID).Spawn(Machine.transform.position);
        ((EntityMachine)Machine).Delete();
    }
}