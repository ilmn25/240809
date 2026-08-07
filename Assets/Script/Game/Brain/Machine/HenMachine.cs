using UnityEngine;

public class HenMachine : AnimalMachine
{
    // Random wait between eggs, in frames: half a day to a day and a half.
    private const int MinLayInterval = Environment.Length * 12;
    private const int MaxLayInterval = Environment.Length * 36;
    private int _nextLayIn;

    protected override string DialogueText => "bawk bawk";

    public static Info CreateInfo()
    {
        return new PassiveInfo()
        {
            HealthMax = 16,
            SpeedGround = 7,
            SpeedAir = 8,
            PathAir = 3,
            DistAttack = 7,
            DistRoam = 3
        };
    }

    public override void OnStart()
    {
        _nextLayIn = Random.Range(MinLayInterval, MaxLayInterval); // first egg after a random wait
        base.OnStart();

        AddState(new MobEscape());
        AddState(new MobChase());
        AddState(new MobAttackPounce(1));
    }

    public override void OnUpdate()
    {
        // Lay an egg, then wait a random amount of time before the next (host only).
        if (_nextLayIn <= 0)
        {
            Entity.SpawnItem(ID.Egg, transform.position);
            _nextLayIn = Random.Range(MinLayInterval, MaxLayInterval);
        }
        else
            _nextLayIn--;

        if (!IsCurrentState<DefaultState>()) return;

        if (Info.Target != null)
        {
            // Retaliate against the player if they attacked us.
            if (Info.Target is PlayerInfo)
                AttackOrChase();
            else if (Vector3.Distance(Info.Target.position, transform.position) < Info.DistAttack)
            {
                if (Random.value < 0.8f)
                    SetState<MobEscape>();
                else
                    SetState<MobRoam>();
            }
            else
                SetState<MobRoam>();
        }
        else
        {
            if (Random.value > 0.5f)
                SetState<MobRoam>();
            else
                SetState<MobIdle>();
        }
    }
}
