using System;

/// <summary>What a status effect does to its host each tick.</summary>
public enum EffectType { Heal, Damage, Slow, MaxHealthPenalty }

/// <summary>Data-driven definition of a status effect. Instances are applied to an
/// entity via StatusEffectModule and tick on the host until their duration expires.</summary>
[Serializable]
public class StatusEffect
{
    public ID EffectID;
    public EffectType Type;
    public float Duration;
    public float TickInterval;
    public int AmountPerTick;
    public float SlowAmount;
    /// <summary>Optional display name shown in the HUD/info panel. Falls back to
    /// the EffectID's display name when empty.</summary>
    public string Name;

    public StatusEffect() { }

    public StatusEffect(ID effectID, EffectType type, float duration, float tickInterval, int amountPerTick = 0, float slowAmount = 0, string name = null)
    {
        EffectID = effectID;
        Type = type;
        Duration = duration;
        TickInterval = tickInterval;
        AmountPerTick = amountPerTick;
        SlowAmount = slowAmount;
        Name = name;
    }
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
        ID.Stagger, EffectType.Slow, duration: 2.5f, tickInterval: 1f, slowAmount: 0.4f, name: "Stagger");
}
