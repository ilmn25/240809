 
using UnityEngine;

public class SheepMachine : AnimalMachine
{
    // Shared scratch buffer for herd scans (results are consumed before the next call).
    private static readonly Collider[] SheepScanBuffer = new Collider[16];

    protected override string DialogueText => "baaaa";

    public static Info CreateInfo()
    {
        return new SheepInfo()
        {
            HealthMax = 16,
            SpeedGround = 6,
            SpeedAir = 6,
            PathAir = 3,
            DistAttack = 5,    // how close the player must be before the flock flees
            DistAlert = 14,    // how far a sheep will follow its flockmates
            DistFollow = 2.5f, // herd spacing: how tight the flock clusters together
            DistRoam = 3,
        };
    }

    public override void OnStart()
    {
        base.OnStart();

        AddState(new MobChaseAction());
        AddState(new MobChase());
        AddState(new MobAttackPounce(1));
        AddState(new MobEscape());
    }

    public override void OnUpdate()
    {
        bool playerAlive = Main.PlayerInfo != null && !Main.PlayerInfo.Destroyed;
        float playerDist = playerAlive
            ? Vector3.Distance(Main.PlayerInfo.position, transform.position)
            : float.MaxValue;

        // Sheep are docile — they never attack on sight. They stay with the flock,
        // and only flee if the player crowds them or something scares them.
        // But if the player attacks a sheep, the whole herd retaliates.
        if (!IsCurrentState<DefaultState>()) return;

        SheepInfo sheepInfo = (SheepInfo)Info;

        if (sheepInfo.Retaliating && Info.Target is PlayerInfo)
        {
            // Retaliate against the player who attacked us (or a flockmate).
            RallyHerd();
            AttackOrChase();
        }
        else if (playerAlive && playerDist < Info.DistAttack)
        {
            if (Info.Target is not PlayerInfo)
                Info.Target = Main.PlayerInfo; // flee anchor
            Scatter();
        }
        else if (Info.Target != null &&
                 Vector3.Distance(Info.Target.position, transform.position) < Info.DistAttack)
        {
            Scatter(); // spooked by something else — run from it
        }
        else if (Info.Target != null)
        {
            // Player backed off — stop retaliating.
            sheepInfo.Retaliating = false;
            SetState<MobRoam>();
        }
        else
        {
            // Stay with the flock; otherwise graze in place.
            if (HerdUp()) return;
            if (Random.value > 0.5f)
                SetState<MobRoam>();
            else
                SetState<MobIdle>();
        }
    }

    // When one sheep is attacked, the whole nearby herd turns on the attacker.
    private void RallyHerd()
    {
        if (Info.Target is not PlayerInfo player) return;
        int count = Physics.OverlapSphereNonAlloc(transform.position, Info.DistAlert, SheepScanBuffer, Main.MaskEntity);
        for (int i = 0; i < count; i++)
        {
            if (SheepScanBuffer[i].TryGetComponent(out SheepMachine other) && other != this &&
                other.Info.Target is not PlayerInfo)
            {
                ((SheepInfo)other.Info).Retaliating = true;
                other.Chase(player);
            }
        }
    }

    // Keeps the flock together: follows the nearest sheep that has strayed past herd spacing.
    private bool HerdUp()
    {
        Info flockmate = EntityScan.FindNearest(transform.position, Info.DistAlert, i => i.Machine is SheepMachine s && s != this);
        if (flockmate == null) return false;
        if ((flockmate.position - transform.position).sqrMagnitude < Info.DistFollow * Info.DistFollow) return false; // already huddled

        Info.Target = flockmate;
        Info.ActionType = IActionType.Follow;
        Info.PathingStatus = PathingStatus.Pending;
        SetState<MobChaseAction>();
        return true;
    }

    // Runs away, or occasionally just mills about.
    private void Scatter()
    {
        if (Random.value < 0.9f)
            SetState<MobEscape>();
        else
            SetState<MobRoam>();
    }
} 