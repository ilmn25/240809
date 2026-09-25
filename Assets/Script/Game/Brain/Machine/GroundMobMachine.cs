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

    /// <summary>Cluster around the given caravan wagon as it travels, and leave the
    /// world once the wagon is gone. Call from OnUpdate while a wagon is assigned
    /// (see PassiveNPCMachine and CowMachine).</summary>
    protected void UpdateCaravanFollow(CaravanMachine caravan)
    {
        if (caravan == null || caravan.Info == null || caravan.Info.Destroyed)
        {
            LeaveCaravan();
            return;
        }

        if (Helper.SquaredDistance(caravan.transform.position, transform.position) >
            Info.DistAttack * Info.DistAttack)
        {
            Info.Target = caravan.Info;
            Info.PathingStatus = PathingStatus.Pending;
            if (!IsCurrentState<MobChase>()) SetState<MobChase>();
        }
        else if (Info.Target == caravan.Info)
        {
            // Caught up: drop the wagon as a target and linger in place.
            // CancelTarget (not null) resets pathing, which dereferences Info.Target.
            Info.CancelTarget();
            SetState<MobIdle>();
        }
        else if (IsCurrentState<MobChase>())
            SetState<MobIdle>();
    }

    /// <summary>Removes this mob from the world now that its caravan has left.</summary>
    protected void LeaveCaravan()
    {
        Info.Destroy();
        Unload();
    }
}
