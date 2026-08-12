using System;

/// <summary>What a status effect does to its host each tick.</summary>
public enum EffectType { Heal, Damage, Slow }

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

    public StatusEffect() { }

    public StatusEffect(ID effectID, EffectType type, float duration, float tickInterval, int amountPerTick = 0, float slowAmount = 0)
    {
        EffectID = effectID;
        Type = type;
        Duration = duration;
        TickInterval = tickInterval;
        AmountPerTick = amountPerTick;
        SlowAmount = slowAmount;
    }
}
