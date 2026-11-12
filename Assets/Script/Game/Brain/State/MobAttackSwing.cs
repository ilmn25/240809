using System.Collections;
using UnityEngine;

/// <summary>Melee attack: telegraph, swing, cooldown. Mobs swing the weapon they have
/// equipped (its projectile is spawned through ProjectileSync) or, when handed a natural
/// weapon and nothing usable is in hand, attack with that and plant their feet while doing
/// it.</summary>
class MobAttackSwing : MobState {

    public MobAttackSwing() { updateMode = global::Module.UpdateMode.Everyone; }

    /// <summary>For mobs that attack with a natural weapon instead of an equipped item.</summary>
    public MobAttackSwing(ProjectileInfo naturalWeapon) : this()
    {
        NaturalWeapon = naturalWeapon;
    }

    /// <summary>Natural weapon this mob attacks with (a beast's bite). Used only when
    /// nothing usable is in hand, so a controlled pet that picked a tool up on the cursor
    /// still swings it.</summary>
    public ProjectileInfo NaturalWeapon { get; set; }

    private Item _equipment;
    private CoroutineTask _task;

    /// <summary>True when this swing uses the mob's own body: it was handed a natural
    /// weapon and has no item with a swing of its own equipped.</summary>
    private bool UsesNaturalWeapon => NaturalWeapon != null && Info.Equipment?.Info.ProjectileInfo == null;

    private const float TelegraphTime = 0.1f;
    private const float SwingTime = 0.167f;
    private const float CooldownTime = 1f;

    private void Attack()
    {
        if (UsesNaturalWeapon)
        {
            Projectile.Spawn(Info.SpriteToolTrack.position, Info.AimPosition,
                NaturalWeapon, Info.targetHitboxType, Info);
            return;
        }

        if (_equipment?.ProjectileInfo == null || Info.Equipment == null) return;

        Vector3 direction = Info.GetDirection();
        ProjectileSync.SpawnProjectile(Info,
            Info.SpriteToolTrack.position + direction * _equipment.ProjectileOffset,
            Info.AimPosition,
            _equipment.ProjectileInfo,
            Info.targetHitboxType, Info.Equipment.ID);
    }

    public override void OnEnterState()
    {
        _task?.Stop();
        _task = null;
        Info.Animator.speed = Main.PlayerInfo == Info ? 0.7f : 0.3f;
        _equipment = Info.Equipment?.Info;
        if (_equipment != null)
            Info.SpriteToolEffect.localPosition = new Vector3(0.8f, -0.3f, 0);
        // Armed mobs keep creeping in; natural attackers freeze until the swing is done.
        Info.SpeedModifier = UsesNaturalWeapon ? 0f : 0.25f;
        Info.Animator.Play("EquipSwingTelegraph", 0, 0f);
        if (Helper.IsHost() || Info.IsOwner())
            _task = new CoroutineTask(SwingRoutine());
    }

    private IEnumerator SwingRoutine()
    {
        yield return new WaitForSeconds(TelegraphTime / Info.Animator.speed);
        Info.Animator.speed = 1f;
        Info.Animator.Play("EquipSwing", 0, 0f);
        Attack();

        yield return new WaitForSeconds(SwingTime / Info.Animator.speed);
        Audio.PlaySFX(_equipment?.Sfx ?? SfxID.HitMob);
        Info.Animator.speed = Main.CreativeMode ? 70f : (_equipment?.Speed ?? 1f);
        Info.SpeedModifier = UsesNaturalWeapon ? 0f : 0.8f;
        Info.Animator.Play("EquipSwingCooldown", 0, 0f);

        yield return new WaitForSeconds(CooldownTime / Info.Animator.speed);
        Info.Animator.speed = 1f;
        Info.Animator.Play("EquipIdle", 0, 0f);
        Machine.SetState<DefaultState>();
    }

    public override void OnExitState()
    {
        _task?.Stop();
        _task = null;
        Info.Animator.speed = 1f;
        Info.Animator.Play("EquipIdle", 0, 0f);
        Info.SpeedModifier = 1f;
    }
}
