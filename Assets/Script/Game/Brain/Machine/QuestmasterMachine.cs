using UnityEngine;

/// <summary>A quest-giver NPC. Interact to hear the current quest; when the task
/// is done it accepts the hand-in, drops the reward, and offers the next quest.</summary>
public class QuestmasterMachine : GroundMobMachine, IActionSecondaryInteract
{
    public static Info CreateInfo()
    {
        return new PassiveInfo()
        {
            HealthMax = 50,
            SpeedGround = 5,
            SpeedAir = 6,
            DistRoam = 3,
            IsNPC = true,
            CharSprite = ID.Guide, // reuse the guide sprite for now
        };
    }

    public override void OnStart()
    {
        base.OnStart();

        AddState(new MobIdle(600)); // lingers in place longer than the animals
        AddState(new MobRoam());
        AddState(new MobHit());
        AddState(new MobEscape());
        AddState(new EquipSelectState());

        AddState(new QuestmasterState());
    }

    public void OnActionSecondary(Info info)
    {
        Info.CancelTarget();
        SetState<QuestmasterState>();
    }

    public override void OnUpdate()
    {
        if (!IsCurrentState<DefaultState>()) return;

        if (Info.Target != null)
        {
            if (Vector3.Distance(Info.Target.position, transform.position) > Info.DistDisengage)
                Info.CancelTarget(); // the threat got away — calm down
            else
                SetState<MobEscape>(); // run from the attacker
            return;
        }

        SetState<MobIdle>();
    }

    public void OnDrawGizmos()
    {
        if (Camera.current == Camera.main)
            GetModule<GroundPathingModule>().DrawGizmos();
    }
}
