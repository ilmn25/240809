using UnityEngine;

public class HenMachine : MobMachine, IActionSecondaryInteract
{
    // Frames between dropped eggs (~40 seconds at 60 fps).
    private const int EggLayInterval = 2400;
    private int _eggTimer;

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
        // Hens lay eggs on the ground on a timer (OnUpdate only runs on the host).
        if (++_eggTimer >= EggLayInterval)
        {
            _eggTimer = 0;
            Entity.SpawnItem(ID.Egg, transform.position);
        }

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
