using UnityEngine;

public class MobStatusModule : StatusModule
{
    public Transform Target = null;
    public PathingStatus PathingStatus = PathingStatus.Pathing;
    public Vector3 Direction = Vector3.zero;
    public bool IsGrounded = false;
    
    public MobStatusModule(HitboxType hitBoxType, float health, float defense) : base(hitBoxType, health, defense) { }

    protected override void OnHit(Projectile projectile)
    {
        Audio.PlaySFX("npc_hurt", 0.8f);
        Target = Game.Player.transform;
        PathingStatus = PathingStatus.Reached;
        Machine.GetModule<GroundMovementModule>().KnockBack(projectile.transform.position, projectile.Info.Knockback, true);
    }

    protected override void OnDeath()
    {
        Audio.PlaySFX("player_die", 0.4f);
        Entity.SpawnItem("sand", Vector3Int.FloorToInt(Machine.transform.position));
        ((EntityMachine)Machine).Delete();
    }
}