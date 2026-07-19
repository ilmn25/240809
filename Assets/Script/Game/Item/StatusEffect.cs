using System;

[Serializable]
public class StatusEffect
{
    public ID EffectID;
    public float Duration;
    public float TickInterval;
    public int DamagePerTick;
    public float SlowAmount;
}
