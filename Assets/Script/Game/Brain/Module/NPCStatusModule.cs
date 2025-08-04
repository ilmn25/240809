using UnityEngine;

public class NPCStatusModule : StatusModule
{
    public NPCStatusModule(HitboxType hitBoxType, float health, float defense) : base(hitBoxType, health, defense)
    {
    }

    protected override void OnHit(Projectile projectile)
    {
        Audio.PlaySFX("npc_hurt", 0.8f);
        Machine.GetState<NPCState>().SetState<NPCChase>();
        Machine.GetModule<NPCMovementModule>().KnockBack(projectile.transform.position, projectile.Info.Knockback, true);
    }

    protected override void OnDeath()
    {
        Audio.PlaySFX("player_die", 0.4f);
        Entity.SpawnItem("sand", Vector3Int.FloorToInt(Machine.transform.position));
        ((EntityMachine)Machine).Delete();
    }
}