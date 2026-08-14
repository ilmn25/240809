 
using UnityEngine; 

public class ScoutMachine : GroundMobMachine
{   
    public static Info CreateInfo()
    {
        return new EnemyInfo()
        {
            HealthMax = 12,
            SpeedGround = 4,
            DistAttack = 18,
        }; 
    }
    private int _ammo;
    private const int AmmoMax = 5; 

    public override void OnStart()
    {
        _ammo = AmmoMax;  
        base.OnStart();
        
        AddState(new MobIdle());
        AddState(new MobChaseAim());
        AddState(new MobRoam());
        AddState(new MobHit());
        AddState(new MobAttackReload());
        AddState(new MobAttackShoot());
        AddState(new EquipSelectState()); 
        Info.SetEquipment(new ItemSlot(ID.Pistol));
    }

    public override void OnUpdate()
    {
        UpdateAggro();

        if (IsCurrentState<DefaultState>())
        {
            if (Info.Target != null)
            {
                if (InRangeAndVisible())
                {
                    if (_ammo != 0)
                    {
                        _ammo--;
                        Info.AimPosition = Info.Target.position + 0.3f * Vector3.up;
                        Attack();
                    }
                    else
                    {
                        SetState<MobAttackReload>();
                        _ammo = AmmoMax;
                    }
                }
                else if (Info.PathingStatus == PathingStatus.Stuck)
                {
                    SetState<MobRoam>();
                }
                else
                {
                    SetState<MobChaseAim>();
                }
            }
            else
            {
                if (Random.value > 0.5f)
                    SetState<MobRoam>();
                else
                    SetState<MobIdle>();
            }


            bool InRangeAndVisible()
            {
                Vector3 origin = transform.position + Vector3.up * 0.3f;
                float distance = Vector3.Distance(origin, Info.Target.position);

                // Debug.DrawRay(origin, direction * distance, Color.red, 0.1f); // Lasts 0.1 seconds

                if (distance > Info.DistAttack) return false;

                if (Physics.Raycast(origin, (Info.Target.position - origin).normalized,
                        out RaycastHit hit, distance, Main.MaskMap))
                {
                    return hit.distance >= distance - 0.2f;
                }

                return true;
            }
        }
    }

    /// <summary>Lock onto the nearest player or friendly NPC on sight; release it
    /// once it retreats well out of disengage range. Guards override this to rely
    /// on their camp-based targeting instead.</summary>
    protected virtual void UpdateAggro()
    {
        Info nearest = FindNearestAggroTarget();
        if (nearest != null)
        {
            if (Info.Target != nearest)
            {
                Info.Target = nearest;
                Info.PathingStatus = PathingStatus.Pending;
            }
            return;
        }

        if (Info.Target != null &&
            Vector3.Distance(Info.Target.position, transform.position) > Info.DistDisengage)
            Info.CancelTarget();
    }

    private Info FindNearestAggroTarget()
    {
        Info best = (Main.PlayerInfo != null && !Main.PlayerInfo.Destroyed) ? Main.PlayerInfo : null;
        Info npc = EntityScan.FindNearest(transform.position, Info.DistAlert, i => i is DynamicInfo d && d.IsNPC);
        if (npc != null && (best == null ||
            (npc.position - transform.position).sqrMagnitude < (best.position - transform.position).sqrMagnitude))
            best = npc;
        return best;
    }
} 