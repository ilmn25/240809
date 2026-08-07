 
using UnityEngine;

public class SheepMachine : MobMachine, IActionSecondaryInteract
{   
    // Shared scratch buffer for herd scans (results are consumed before the next call).
    private static readonly Collider[] SheepScanBuffer = new Collider[16];

    public static Info CreateInfo()
    { 
        return new PassiveInfo()
        {
            HealthMax = 16,
            SpeedGround = 7,
            SpeedAir = 8,
            PathAir = 3,
            DistAttack = 5,    // how close the player must be before the flock flees
            DistAlert = 7,     // how far a sheep will follow its flockmates
            DistFollow = 2.5f, // herd spacing: how tight the flock clusters together
            DistRoam = 3,
        };  
    }
    public override void OnStart()
    {
        AddModule(new GroundMovementModule());
        AddModule(new GroundPathingModule());
        AddModule(new GroundAnimationModule());
        AddModule(new MobSpriteCullModule());
        AddModule(new SpriteOrbitModule());

        AddState(new MobIdle());
        AddState(new MobRoam());
        AddState(new MobChaseAction());
        AddState(new MobHit());
        AddState(new EquipSelectState());
        
        Dialogue dialogue = new Dialogue
        {
            Text = "baaaa", 
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
        bool playerAlive = Main.PlayerInfo != null && !Main.PlayerInfo.Destroyed;
        float playerDist = playerAlive
            ? Vector3.Distance(Main.PlayerInfo.position, transform.position)
            : float.MaxValue;

        // Sheep are docile — they never attack on sight. They stay with the flock,
        // and only flee if the player crowds them or something scares them.
        if (!IsCurrentState<DefaultState>()) return;

        if (playerAlive && playerDist < Info.DistAttack)
        {
            if (Info.Target is not PlayerInfo)
                Info.Target = Main.PlayerInfo; // flee anchor
            Scatter();
        }
        else if (Info.Target is PlayerInfo)
        {
            Info.CancelTarget(); // player backed off — stop running
        }
        else if (Info.Target != null &&
                 Vector3.Distance(Info.Target.position, transform.position) < Info.DistAttack)
        {
            Scatter(); // spooked by something else — run from it
        }
        else if (Info.Target != null)
        {
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

    // Keeps the flock together: follows the nearest sheep that has strayed past herd spacing.
    private bool HerdUp()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, Info.DistAlert, SheepScanBuffer, Main.MaskEntity);
        SheepMachine flockmate = null;
        float bestSqr = float.MaxValue;
        for (int i = 0; i < count; i++)
        {
            if (SheepScanBuffer[i].TryGetComponent(out SheepMachine other) && other != this)
            {
                float sqr = (other.transform.position - transform.position).sqrMagnitude;
                if (sqr < bestSqr) { bestSqr = sqr; flockmate = other; }
            }
        }
        if (flockmate == null) return false;
        if (bestSqr < Info.DistFollow * Info.DistFollow) return false; // already huddled

        Info.Target = flockmate.Info;
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

    public void OnDrawGizmos()
    {
        if (Camera.current == Camera.main)
            GetModule<GroundPathingModule>().DrawGizmos();
    }
 
} 