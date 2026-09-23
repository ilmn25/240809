using UnityEngine;

/// <summary>A placid cow that travels with the visiting bandwagon. Spawned by
/// CaravanMachine beside the nomads, it trails the wagon and leaves with it —
/// the animal counterpart of the caravan escort.</summary>
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

            // A real threat outranks the escort wagon.
            if (UpdateThreat()) return;

            UpdateCaravanFollow(Caravan);
            return;
        }

        // Left to itself the cow is docile: it grazes, and only runs from a threat.
        if (!IsCurrentState<DefaultState>()) return;

        if (UpdateThreat()) return;

        if (Random.value > 0.5f)
            SetState<MobRoam>();
        else
            SetState<MobIdle>();
    }

    /// <summary>Run from a real threat, and settle down once it gets away. Returns true
    /// while a threat is being handled — the escort wagon doesn't count.</summary>
    private bool UpdateThreat()
    {
        if (Info.Target == null || (Caravan != null && Info.Target == Caravan.Info)) return false;

        if (Vector3.Distance(Info.Target.position, transform.position) > Info.DistDisengage)
            Info.CancelTarget(); // the threat got away — settle down
        else
            SetState<MobEscape>();

        return true;
    }
}
