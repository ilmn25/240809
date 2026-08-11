 
using UnityEngine;
using Random = UnityEngine.Random;

public class HarpyMachine : MobMachine
{
    private const int StalkDistance = 6;   // how close a lone harpy creeps before stopping
    private const int GroupAttackCount = 3; // harpies needed before they dive in
    private const float GroupRadius = 8f;   // how close harpies must be to count as grouped

    private static readonly Collider[] HarpyScanBuffer = new Collider[16];

    public static Info CreateInfo()
    {
        return new EnemyInfo()
        {
            HealthMax = 16,
            DistAttack = 1,
            DistRoam = 7,
            PathJump = 10,
            PathAir = -1,
            SpeedAir = 7,
            JumpVelocity = 7,
            CanFly = true,
        };
    }

    public override void OnStart()
    {
        AddModule(new GroundMovementModule());
        AddModule(new GroundPathingModule());
        AddModule(new GroundAnimationModule());
        AddModule(new MobSpriteCullModule());
        AddModule(new SpriteOrbitModule());
        AddModule(new DoorBashModule());

        AddState(new MobIdle());
        AddState(new MobChase());
        AddState(new MobStalk(StalkDistance));
        AddState(new MobRoam());
        AddState(new MobEvade());
        AddState(new MobHit());
        AddState(new MobAttackSwing());
        AddState(new EquipSelectState());
        Info.SetEquipment(new ItemSlot(ID.SteelSword));
    }

    public override void OnUpdate()
    {
        HandleInput();

        if (IsCurrentState<DefaultState>())
        {
            if (Info.Target != null)
            {
                float dist = Vector3.Distance(Info.Target.position, transform.position);

                // Not enough harpies grouped up yet — stalk from a distance.
                if (GroupedHarpies() < GroupAttackCount)
                {
                    if (dist < StalkDistance)
                    {
                        // Too close while stalking: back off to keep the stalk distance.
                        SetState<MobEvade>();
                    }
                    else if (Info.PathingStatus == PathingStatus.Stuck)
                    {
                        SetState<MobRoam>();
                    }
                    else
                    {
                        SetState<MobStalk>();
                    }
                }
                // Enough harpies grouped — dive in and attack.
                else if (dist < Info.DistAttack)
                {
                    if (Random.value < 0.7f)
                    {
                        Info.AimPosition = Info.Target.position;
                        Attack();
                    }
                    else
                        SetState<MobEvade>();
                }
                else if (Info.PathingStatus == PathingStatus.Stuck)
                {
                    SetState<MobRoam>();
                }
                else
                {
                    SetState<MobChase>();
                }
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

    /// <summary>Counts how many harpies are grouped near this one (including itself).</summary>
    private int GroupedHarpies()
    {
        int count = 1;
        int hits = Physics.OverlapSphereNonAlloc(transform.position, GroupRadius, HarpyScanBuffer, Main.MaskEntity);
        for (int i = 0; i < hits; i++)
        {
            if (HarpyScanBuffer[i].TryGetComponent(out HarpyMachine other) && other != this)
                count++;
        }
        return count;
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            Info.Target = Main.PlayerInfo;
            Info.PathingStatus = PathingStatus.Reached;
            SetState<DefaultState>();
        }
        else if (Input.GetKeyDown(KeyCode.T))
            Info.Target = null;
        else if (Input.GetKeyDown(KeyCode.U))
            transform.position = Main.Player.transform.position;
    }

    public void OnDrawGizmos()
    {
        if (Camera.current != Camera.main)
            return;

        GetModule<GroundPathingModule>().DrawGizmos();
    }
} 