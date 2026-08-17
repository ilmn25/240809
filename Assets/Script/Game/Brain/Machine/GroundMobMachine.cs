using UnityEngine;

/// <summary>Base for ground-based mobs (animals, NPCs, enemies) that share the
/// standard movement/pathing/animation/sprite module stack. Subclasses call
/// base.OnStart() and register their own states. Hostile melee mobs opt into
/// door-bashing via UsesDoorBash.</summary>
public abstract class GroundMobMachine : MobMachine
{
    /// <summary>Whether this mob bashes doors/barricades blocking its path.</summary>
    protected virtual bool UsesDoorBash => false;

    public override void OnStart()
    {
        AddModule(new GroundMovementModule());
        AddModule(new GroundPathingModule());
        AddModule(new GroundAnimationModule());
        AddModule(new MobSpriteCullModule());
        AddModule(new SpriteOrbitModule());
        if (UsesDoorBash)
            AddModule(new DoorBashModule());
    }

    public void OnDrawGizmos()
    {
        if (Camera.current != Camera.main)
            return;
        GetModule<GroundPathingModule>().DrawGizmos();
    }

    /// <summary>Lock onto the nearest player or friendly NPC on sight; release it
    /// once it retreats well out of disengage range. Hostile mobs call this from
    /// their OnUpdate. Guards use this too; their extra camp leash is in GuardModule.</summary>
    protected virtual void UpdateAggro()
    {
        Info nearest = FindNearestAggroTarget();
        if (nearest != null)
        {
            if (Info.Target != nearest)
            {
                Info.Target = nearest;
                Info.PathingStatus = PathingStatus.Pending;
            }
            return;
        }

        if (Info.Target != null &&
            Vector3.Distance(Info.Target.position, transform.position) > Info.DistDisengage)
            Info.CancelTarget();
    }

    /// <summary>Nearest player or friendly NPC within alert range.</summary>
    private Info FindNearestAggroTarget()
    {
        Info best = (Main.PlayerInfo != null && !Main.PlayerInfo.Destroyed &&
                     Vector3.Distance(Main.PlayerInfo.position, transform.position) <= Info.DistAlert)
            ? Main.PlayerInfo : null;
        Info npc = EntityScan.FindNearest(transform.position, Info.DistAlert, i => i is DynamicInfo d && d.IsNPC);
        if (npc != null && (best == null ||
            (npc.position - transform.position).sqrMagnitude < (best.position - transform.position).sqrMagnitude))
            best = npc;
        return best;
    }
}
