using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>A crazed acolyte wielding a burning torch. Like the congregant, it
/// aggroes on sight and chases the player, but its torch attack sets the player
/// on fire instead of dealing direct damage.</summary>
public class AcolyteMachine : GroundMobMachine
{
    protected override bool UsesDoorBash => true;

    public static Info CreateInfo()
    {
        return new EnemyInfo()
        {
            HealthMax = 18,
            DistAlert = 12,
            DistDisengage = 18,
            DistRoam = 6,
            SpeedGround = 4,
            SpeedAir = 5,
        };
    }

    public override void OnStart()
    {
        base.OnStart();

        AddState(new MobIdle());
        AddState(new MobChase());
        AddState(new MobRoam());
        AddState(new MobEvade());
        AddState(new MobHit());
        AddState(new MobAttackSwing());
        AddState(new EquipSelectState());

        Info.SetEquipment(new ItemSlot(ID.Torch));
    }

    public override void OnUpdate()
    {
        UpdateAggro();

        if (IsCurrentState<DefaultState>())
        {
            if (Info.Target != null)
            {
                if (Vector3.Distance(Info.Target.position, transform.position) < Info.DistAttack)
                {
                    Info.AimPosition = Info.Target.position;
                    Attack();
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
}
