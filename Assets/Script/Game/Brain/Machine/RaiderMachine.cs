 
using UnityEngine;

public class RaiderMachine : GroundMobMachine
{
    protected override bool UsesDoorBash => true;

    public static Info CreateInfo()
    { 
        return new EnemyInfo()
        {
            HealthMax = 16,
            DistRoam = 7,
            DistAlert = 12,
            DistDisengage = 18
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

        Info.SetEquipment(new ItemSlot(ID.SteelSword));
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
                    if (Random.value < 0.9f)
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
     
} 