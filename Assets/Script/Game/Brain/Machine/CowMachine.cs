using UnityEngine;

/// <summary>A placid cow that travels with the visiting bandwagon. Spawned by
/// CaravanMachine beside the nomads, it trails the wagon and leaves with it —
/// the animal counterpart of PassiveNPCMachine.UpdateCaravanFollow.</summary>
public class CowMachine : AnimalMachine
{
    /// <summary>The wagon this cow belongs to, set by CaravanMachine when it
    /// spawns its camp. While set, the cow trails the wagon and leaves when the
    /// wagon flees or despawns.</summary>
    public CaravanMachine Caravan;

    protected override string DialogueText => "moo";

    public static Info CreateInfo()
    {
        return new PassiveInfo()
        {
            HealthMax = 20,
            SpeedGround = 5,
            SpeedAir = 5,
            PathAir = 3,
            DistAttack = 2, // how close the cow trails the wagon before settling
            DistRoam = 3,
        };
    }

    public override void OnStart()
    {
        base.OnStart();

        AddState(new MobChase());
        AddState(new MobEscape());
    }

    public override void OnUpdate()
    {
        // A caravan cow trails the wagon and leaves when it's gone.
        if (Caravan != null)
        {
            // Don't interrupt a hit reaction.
            if (IsCurrentState<MobHit>()) return;
            UpdateCaravanFollow();
            return;
        }

        // Left to itself the cow is docile: it grazes, and only runs from a threat.
        if (!IsCurrentState<DefaultState>()) return;

        if (Info.Target != null)
        {
            if (Vector3.Distance(Info.Target.position, transform.position) > Info.DistDisengage)
                Info.CancelTarget(); // the threat got away — settle down
            else
                SetState<MobEscape>();
            return;
        }

        if (Random.value > 0.5f)
            SetState<MobRoam>();
        else
            SetState<MobIdle>();
    }

    /// <summary>Cluster around the wagon as it travels, and leave once it's gone.</summary>
    private void UpdateCaravanFollow()
    {
        if (Caravan.Info == null || Caravan.Info.Destroyed)
        {
            Info.Destroy();
            Unload();
            return;
        }

        if (Vector3.Distance(Caravan.transform.position, transform.position) > Info.DistAttack)
        {
            Info.Target = Caravan.Info;
            Info.PathingStatus = PathingStatus.Pending;
            if (!IsCurrentState<MobChase>()) SetState<MobChase>();
        }
        else if (Info.Target == Caravan.Info)
        {
            // Caught up: drop the wagon as a target and linger in place.
            // CancelTarget (not null) resets pathing, which dereferences Info.Target.
            Info.CancelTarget();
            SetState<MobIdle>();
        }
        else if (IsCurrentState<MobChase>())
            SetState<MobIdle>();
    }
}
