using UnityEngine;

public class HenMachine : MobMachine, IActionSecondaryInteract
{
    // Random wait between eggs, in frames: half a day to a day and a half.
    private const int MinLayInterval = Environment.Length * 12;
    private const int MaxLayInterval = Environment.Length * 36;
    private int _nextLayIn;

    public static Info CreateInfo()
    {
        return new EnemyInfo()
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

        AddModule(new GroundMovementModule());
        AddModule(new GroundPathingModule());
        AddModule(new GroundAnimationModule());
        AddModule(new MobSpriteCullModule());
        AddModule(new SpriteOrbitModule());

        AddState(new MobIdle());
        AddState(new MobRoam());
        AddState(new MobEscape());
        AddState(new MobHit());
        AddState(new EquipSelectState());

        Dialogue dialogue = new Dialogue
        {
            Text = "bawk bawk",
        };
        AddState(new DialogueState(dialogue));
    }

    public void OnActionSecondary(Info info)
    {
        if (Info.Target != null) return;
        SetState<DialogueState>();
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
            if (Vector3.Distance(Info.Target.position, transform.position) < Info.DistAttack)
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

    public void OnDrawGizmos()
    {
        if (Camera.current == Camera.main)
            GetModule<GroundPathingModule>().DrawGizmos();
    }
}
