using System;
using UnityEngine;

/// <summary>What a status effect does to its host each tick.</summary>
public enum EffectType { Heal, Damage, Slow, Haste, MaxHealthPenalty }

/// <summary>Data-driven definition of a status effect. Instances are applied to an
/// entity via StatusEffectModule and tick on the host until their duration expires.
/// An effect can also drive a looping particle visual anchored to its host, which is
/// what makes fire (or any future effect) follow whatever it is affecting.</summary>
[Serializable]
public class StatusEffect
{
    public ID EffectID;
    public EffectType Type;
    public float Duration;
    public float TickInterval;
    public int AmountPerTick;
    /// <summary>Fractional speed change: Slow reduces speed by this much, Haste raises it.</summary>
    public float SpeedAmount;
    /// <summary>Optional display name shown in the HUD/info panel. Falls back to
    /// the EffectID's display name when empty.</summary>
    public string Name;

    /// <summary>Particle drawn on the affected entity while the effect is active.
    /// None means the effect has no art.</summary>
    public Particles Visual = Particles.None;
    /// <summary>Seconds between visual emissions.</summary>
    public float VisualInterval = 0.4f;
    /// <summary>Offset from the affected entity's origin for the visual.</summary>
    public Vector3 VisualOffset = new Vector3(0f, 0.5f, 0f);

    public StatusEffect() { }

    public StatusEffect(ID effectID, EffectType type, float duration, float tickInterval, int amountPerTick = 0, float speedAmount = 0, string name = null)
    {
        EffectID = effectID;
        Type = type;
        Duration = duration;
        TickInterval = tickInterval;
        AmountPerTick = amountPerTick;
        SpeedAmount = speedAmount;
        Name = name;
    }

    /// <summary>Does the effect do anything on tick, or is it purely a modifier?</summary>
    public bool Ticks => Type == EffectType.Heal || Type == EffectType.Damage;

    /// <summary>Whether the effect draws a particle while active.</summary>
    public bool HasVisual => Visual != Particles.None;
}

/// <summary>Well-known status effects shared by the fire/combat systems.</summary>
public static class StatusEffects
{
    /// <summary>Burning: damage over time with flames that track the burning entity.
    /// FlammableModule uses this for both the fire's lifetime and its visuals.</summary>
    public static readonly StatusEffect Burning = new StatusEffect(
        ID.Burn, EffectType.Damage, duration: 8f, tickInterval: 1f, amountPerTick: 1, name: "Burning")
    {
        Visual = Particles.Fire,
        VisualInterval = 0.35f,
    };
}

/// <summary>On-hit status effects carried by weapons. Shared instances: re-applying the same
/// effect refreshes its duration rather than stacking (see StatusEffectModule).</summary>
public static class WeaponEffects
{
    /// <summary>Damage over time from a wound that keeps reopening.</summary>
    public static readonly StatusEffect Bleeding = new StatusEffect(
        ID.Bleed, EffectType.Damage, duration: 6f, tickInterval: 2f, amountPerTick: 1, name: "Bleeding");

    /// <summary>Slows what it hits, so a heavy landing buys the swinger room.</summary>
    public static readonly StatusEffect Stagger = new StatusEffect(
        ID.Stagger, EffectType.Slow, duration: 2.5f, tickInterval: 1f, speedAmount: 0.4f, name: "Stagger");
}
