using UnityEngine;

public class ChickMachine : AnimalMachine
{
    protected override string DialogueText => "peep peep";

    public static Info CreateInfo()
    {
        return new ChickInfo()
        {
            HealthMax = 8,     // fragile baby chick
            SpeedGround = 8,   // quick to scurry
            SpeedAir = 9,
            PathAir = 3,
            DistAlert = 10,    // how far a chick will follow a hen
            DistFollow = 1.5f, // how close chicks huddle to a hen
            DistRoam = 3
        };
    }

    public override void OnStart()
    {
        base.OnStart();
        AddState(new MobChaseAction());
    }

    public override void OnUpdate()
    {
        if (!IsCurrentState<DefaultState>()) return;

        // Chicks just trail the nearest hen; otherwise they wander nearby.
        if (FollowHen()) return;

        if (Info.Target != null)
            Info.CancelTarget(); // stale target after being hit — rejoin the hen

        if (Random.value > 0.5f)
            SetState<MobRoam>();
        else
            SetState<MobIdle>();
    }

    // Baby chicks trail the nearest hen when nothing is spooking them.
    private bool FollowHen()
    {
        Info hen = EntityScan.FindNearest(transform.position, Info.DistAlert, i => i.Machine is HenMachine);
        if (hen == null) return false;
        if ((hen.position - transform.position).sqrMagnitude < Info.DistFollow * Info.DistFollow) return false; // already huddled by the hen

        Info.Target = hen;
        Info.ActionType = IActionType.Follow;
        Info.PathingStatus = PathingStatus.Pending;
        SetState<MobChaseAction>();
        return true;
    }
}
