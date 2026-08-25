using UnityEngine;

/// <summary>A stationary gun turret — a scout stripped of all movement AI. It stays
/// put, trains its pistol on a player in range and line of sight, fires a magazine,
/// then reloads in place when it runs dry.</summary>
public class TurretMachine : MobMachine
{
    private const int AmmoMax = 5;

    private int _ammo;

    public static Info CreateInfo()
    {
        return new EnemyInfo()
        {
            HealthMax = 20,
            DistAttack = 18,
            DistAlert = 20,
            CharSprite = ID.Turret,
        };
    }

    public override void OnStart()
    {
        _ammo = AmmoMax;
        AddModule(new SpriteOrbitModule());
        AddModule(new MobSpriteCullModule());
        AddModule(new GroundAnimationModule());
        AddState(new MobIdle());
        AddState(new MobHit());
        AddState(new TurretReloadState());
        AddState(new MobAttackShoot());
        AddState(new EquipSelectState());
        Info.SetEquipment(new ItemSlot(ID.Pistol));
    }

    public override void OnUpdate()
    {
        if (!Helper.IsHost()) return;
        UpdateTarget();
        if (!IsCurrentState<DefaultState>() || Info.Target == null) return;

        if (!InRangeAndVisible()) return;

        if (_ammo != 0)
        {
            _ammo--;
            Info.AimPosition = Info.Target.position + 0.3f * Vector3.up;
            Attack();
        }
        else
        {
            _ammo = AmmoMax;
            SetState<TurretReloadState>();
        }
    }

    private void UpdateTarget()
    {
        bool inAlert = Main.PlayerInfo != null && !Main.PlayerInfo.Destroyed &&
                       Vector3.Distance(Main.PlayerInfo.position, transform.position) <= Info.DistAlert;
        Info.Target = inAlert ? Main.PlayerInfo : null;
    }

    private bool InRangeAndVisible()
    {
        Vector3 origin = transform.position + Vector3.up * 0.3f;
        float distance = Vector3.Distance(origin, Info.Target.position);
        if (distance > Info.DistAttack) return false;

        if (Physics.Raycast(origin, (Info.Target.position - origin).normalized,
                out RaycastHit hit, distance, Main.MaskMap))
            return hit.distance >= distance - 0.2f;

        return true;
    }
}
