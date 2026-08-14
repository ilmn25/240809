using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>A cultist necromancer (like the Clash Royale necromancer). It stays
/// back and keeps summoning thralls while the player is in range. It never chases
/// — it holds position and raises thralls, backing off only if the player gets
/// too close. Killing the cultist stops the summons.</summary>
public class CultistMachine : GroundMobMachine
{
    private const int SummonInterval = 180;   // frames between summons (~3s)
    private const int MaxThralls = 5;         // how many thralls it keeps alive
    private const float SummonRange = 12f;    // how close the player must be to summon
    private const float KeepDistance = 6f;    // how far it tries to stay from the player

    private int _summonTimer;

    public static Info CreateInfo()
    {
        return new EnemyInfo()
        {
            HealthMax = 30,
            DistAlert = 16,
            DistDisengage = 20,
            DistRoam = 5,
            SpeedGround = 3,
            SpeedAir = 4,
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
        AddState(new EquipSelectState());
    }

    public override void OnUpdate()
    {
        UpdateAggro();

        if (IsCurrentState<DefaultState>())
        {
            if (Info.Target == null)
            {
                if (Random.value > 0.5f)
                    SetState<MobRoam>();
                else
                    SetState<MobIdle>();
                return;
            }

            float dist = Vector3.Distance(Info.Target.position, transform.position);

            // Player is in range — stay back and summon.
            if (dist < SummonRange)
            {
                // Back off if the player gets too close, otherwise hold and summon.
                if (dist < KeepDistance)
                {
                    SetState<MobEvade>();
                }
                else
                {
                    Info.Direction = Vector3.zero;
                    if (++_summonTimer >= SummonInterval)
                    {
                        _summonTimer = 0;
                        if (CountThralls() < MaxThralls)
                            SummonThrall();
                    }
                }
                return;
            }

            // Player out of range — hold position (don't chase).
            Info.Direction = Vector3.zero;
            SetState<MobIdle>();
        }
    }

    /// <summary>Raise a thrall beside the cultist.</summary>
    private void SummonThrall()
    {
        Vector3Int spawnPos = Vector3Int.FloorToInt(transform.position) + new Vector3Int(1, 1, 0);
        Entity.Spawn(ID.Thrall, spawnPos);
        Particle.Create(transform.position + Vector3.up, Particles.HitDust, true);
        Audio.PlaySFX(SfxID.Notification);
    }

    /// <summary>Count how many thralls are currently alive.</summary>
    private int CountThralls()
    {
        int count = 0;
        foreach (var em in EntityDynamicLoad.ActiveEntities)
        {
            if (em != null && em is ThrallMachine && !em.Info.Destroyed)
                count++;
        }
        return count;
    }
}
