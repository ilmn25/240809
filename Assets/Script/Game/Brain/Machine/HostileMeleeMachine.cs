using System;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>Per-id combat data for <see cref="HostileMeleeMachine"/>. Registered with the
/// entity in <see cref="Entity.Initialize"/>, so an enemy is a row of data — stats, weapon
/// or natural projectile, dodge chance — instead of its own machine class.</summary>
public sealed class MeleeEnemy
{
    /// <summary>Weapon the mob swings. <see cref="ID.Null"/> when it attacks with
    /// <see cref="Projectile"/> instead.</summary>
    public ID Weapon = ID.Null;

    /// <summary>Natural weapon used instead of an equipped item.</summary>
    public ProjectileInfo Projectile;

    /// <summary>Chance to dodge back instead of swinging when the target is in range.</summary>
    public float EvadeChance;

    /// <summary>Extra setup the mob needs, such as an additional module.</summary>
    public Action<HostileMeleeMachine> Setup;
}

/// <summary>Hostile melee mob: aggroes on sight, chases the target down and freezes to
/// swing when it closes to melee range. Every armed brute, beast and boss shares this one
/// brain; what differs between them is the registered <see cref="MeleeEnemy"/> data.</summary>
public class HostileMeleeMachine : GroundMobMachine
{
    protected override bool UsesDoorBash => true;

    private MeleeEnemy _melee;

    public override void OnStart()
    {
        base.OnStart();

        _melee = Entity.Dictionary.TryGetValue(Info.id, out Entity data) ? data.Melee : null;

        AddState(new MobIdle());
        AddState(new MobChase());
        AddState(new MobRoam());
        AddState(new MobHit());
        AddState(new MobAttackSwing(_melee?.Projectile));
        AddState(new EquipSelectState());
        if (_melee != null && _melee.EvadeChance > 0f)
            AddState(new MobEvade());

        if (_melee != null && _melee.Weapon != ID.Null)
            Info.SetEquipment(new ItemSlot(_melee.Weapon));

        _melee?.Setup?.Invoke(this);
    }

    public override void OnUpdate()
    {
        UpdateAggro();
        if (!IsCurrentState<DefaultState>()) return;

        // Dodge back instead of swinging every so often.
        if (_melee != null && _melee.EvadeChance > 0f && Info.Target != null &&
            Vector3.Distance(Info.Target.position, transform.position) < Info.DistAttack &&
            Random.value < _melee.EvadeChance)
        {
            SetState<MobEvade>();
            return;
        }

        if (!EngageTarget()) RoamOrIdle();
    }
}
