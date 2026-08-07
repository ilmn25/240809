using UnityEngine;

public class ChickMachine : AnimalMachine
{
    // Shared scratch buffer for hen scans (results are consumed before the next call).
    private static readonly Collider[] FollowScanBuffer = new Collider[16];

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
        int count = Physics.OverlapSphereNonAlloc(transform.position, Info.DistAlert, FollowScanBuffer, Main.MaskEntity);
        HenMachine hen = null;
        float bestSqr = float.MaxValue;
        for (int i = 0; i < count; i++)
        {
            if (FollowScanBuffer[i].TryGetComponent(out HenMachine other))
            {
                float sqr = (other.transform.position - transform.position).sqrMagnitude;
                if (sqr < bestSqr) { bestSqr = sqr; hen = other; }
            }
        }
        if (hen == null) return false;
        if (bestSqr < Info.DistFollow * Info.DistFollow) return false; // already huddled by the hen

        Info.Target = hen.Info;
        Info.ActionType = IActionType.Follow;
        Info.PathingStatus = PathingStatus.Pending;
        SetState<MobChaseAction>();
        return true;
    }
}
