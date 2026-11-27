using System.Collections;
using UnityEngine;

class BaelHellfire : MobState
{
    private const int Bolts = 5;
    private const float SpreadDegrees = 14f;
    private const float TelegraphTime = 0.45f;
    private const float RecoverTime = 0.35f;

    private static readonly RangedProjectileInfo Bolt = new RangedProjectileInfo
    {
        Damage = 4,
        Knockback = 8,
        CritChance = 10,
        Radius = 0.3f,
        Speed = 16f,
        LifeSpan = 240,
        Sprite = ID.FlameArrow,
        HitEffect = StatusEffects.Burning,
        Class = ProjectileClass.Magic,
    };

    private CoroutineTask _task;

    public override void OnEnterState()
    {
        Info.SpeedModifier = 0f;
        Info.Animator.speed = 1f;
        Info.Animator.Play("EquipShoot", 0, 0f);
        if (Helper.IsHost() || Info.IsOwner())
            _task = new CoroutineTask(Routine());
    }

    private IEnumerator Routine()
    {
        yield return new WaitForSeconds(TelegraphTime);
        Fire();
        yield return new WaitForSeconds(RecoverTime);
        Finish();
    }

    private void Fire()
    {
        Vector3 origin = Info.SpriteToolTrack.position;
        Vector3 direction = Info.GetAimPoint(origin, Bolt) - origin;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f) direction = Info.GetDirection();
        else direction.Normalize();

        Audio.PlaySFX(SfxID.Noise);
        Particle.Create(origin, Particles.Fire, false);

        for (int i = 0; i < Bolts; i++)
        {
            float angle = (i - (Bolts - 1) * 0.5f) * SpreadDegrees;
            Vector3 dir = Quaternion.Euler(0f, angle, 0f) * direction;
            Projectile.Spawn(origin, origin + dir * 10f, Bolt, HitboxType.Player, Info);
        }
    }

    private void Finish()
    {
        Info.Animator.Play("EquipIdle", 0, 0f);
        Machine.SetState<DefaultState>();
    }

    public override void OnExitState()
    {
        _task?.Stop();
        _task = null;
        Info.SpeedModifier = 1f;
    }
}

class BaelCharge : MobState
{
    private const float TelegraphTime = 0.5f;
    private const float DashTime = 0.55f;
    private const float DashSpeed = 13f;
    private const int TrampleInterval = 4;

    private static readonly ProjectileInfo Trample = new ContactDamageProjectileInfo
    {
        Damage = 6,
        Knockback = 24,
        CritChance = 10,
        Radius = 0.9f,
    };

    private CoroutineTask _task;

    public override void OnEnterState()
    {
        Info.SpeedModifier = 0f;
        Info.Animator.speed = 1f;
        Info.Animator.Play("EquipSwingTelegraph", 0, 0f);
        if (Helper.IsHost() || Info.IsOwner())
            _task = new CoroutineTask(Routine());
    }

    private IEnumerator Routine()
    {
        yield return new WaitForSeconds(TelegraphTime);
        if (Info.Target == null)
        {
            Finish();
            yield break;
        }

        Vector3 direction = Info.Target.position - Machine.transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f) direction = Info.GetDirection();
        else direction.Normalize();

        Module<PathingModule>().SetTarget(PathingTarget.None);
        Info.Animator.Play("EquipSwing", 0, 0f);
        Info.SpeedModifier = 1f;
        Audio.PlaySFX(SfxID.Noise);

        float elapsed = 0f;
        int frame = 0;
        while (elapsed < DashTime)
        {
            Info.Direction = direction;
            Info.SpeedCurrent = DashSpeed;
            if (Helper.IsHost() && frame++ % TrampleInterval == 0)
                Projectile.Spawn(Info.SpriteToolTrack.position,
                    Info.SpriteToolTrack.position + direction, Trample, HitboxType.Player, Info);
            elapsed += Time.deltaTime;
            yield return null;
        }

        Finish();
    }

    private void Finish()
    {
        Info.Direction = Vector3.zero;
        Info.SpeedCurrent = 0f;
        Info.Animator.Play("EquipIdle", 0, 0f);
        Machine.SetState<DefaultState>();
    }

    public override void OnExitState()
    {
        _task?.Stop();
        _task = null;
        Info.Direction = Vector3.zero;
        Info.SpeedModifier = 1f;
    }
}

class BaelNova : MobState
{
    private const float TelegraphTime = 0.8f;
    private const float RecoverTime = 0.4f;

    private static readonly ProjectileInfo Shockwave = new ContactDamageProjectileInfo
    {
        Damage = 9,
        Knockback = 30,
        CritChance = 5,
        Radius = 3.2f,
    };

    private CoroutineTask _task;

    public override void OnEnterState()
    {
        Info.SpeedModifier = 0f;
        Info.Animator.speed = 1f;
        Info.Animator.Play("EquipSwingTelegraph", 0, 0f);
        Audio.PlaySFX(SfxID.Thunder);
        Particle.CreateAttached((EntityMachine)Machine, Vector3.up * 0.5f, Particles.Fire, false);
        if (Helper.IsHost() || Info.IsOwner())
            _task = new CoroutineTask(Routine());
    }

    private IEnumerator Routine()
    {
        yield return new WaitForSeconds(TelegraphTime);
        Detonate();
        yield return new WaitForSeconds(RecoverTime);
        Finish();
    }

    private void Detonate()
    {
        Vector3 position = Machine.transform.position;
        Audio.PlaySFX(SfxID.Thunder);
        ScreenShake.Shake(80f, 0.5f, 0.6f);
        if (!Helper.IsHost()) return;

        Projectile.Spawn(position, position + Vector3.forward, Shockwave, HitboxType.Player, Info);
        Particle.Create(position, Particles.HitDust, true);
        Particle.Create(position, Particles.Fire, true);
    }

    private void Finish()
    {
        Info.Animator.Play("EquipIdle", 0, 0f);
        Machine.SetState<DefaultState>();
    }

    public override void OnExitState()
    {
        _task?.Stop();
        _task = null;
        Info.SpeedModifier = 1f;
    }
}

class BaelSummon : MobState
{
    private const float TelegraphTime = 0.7f;
    private const float RecoverTime = 0.4f;
    private const int Minions = 2;

    private CoroutineTask _task;

    public override void OnEnterState()
    {
        Info.SpeedModifier = 0f;
        Info.Animator.speed = 1f;
        Info.Animator.Play("EquipCraft", 0, 0f);
        Audio.PlaySFX(SfxID.Notification);
        if (Helper.IsHost() || Info.IsOwner())
            _task = new CoroutineTask(Routine());
    }

    private IEnumerator Routine()
    {
        yield return new WaitForSeconds(TelegraphTime);
        Summon();
        yield return new WaitForSeconds(RecoverTime);
        Finish();
    }

    private void Summon()
    {
        if (!Helper.IsHost()) return;

        for (int i = 0; i < Minions; i++)
        {
            float angle = i * Mathf.PI * 2f / Minions;
            Vector3Int cell = Vector3Int.FloorToInt(Machine.transform.position +
                new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * 2f);
            if (!NavMap.TryFindOpenSpot(cell, 2, out Vector3Int spot)) continue;

            Entity.Spawn(ID.Thrall, spot);
            Particle.Create(new Vector3(spot.x + 0.5f, spot.y, spot.z + 0.5f), Particles.HitDust, true);
        }
    }

    private void Finish()
    {
        Info.Animator.Play("EquipIdle", 0, 0f);
        Machine.SetState<DefaultState>();
    }

    public override void OnExitState()
    {
        _task?.Stop();
        _task = null;
        Info.SpeedModifier = 1f;
    }
}
