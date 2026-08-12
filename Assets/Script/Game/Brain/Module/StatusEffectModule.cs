using System.Collections.Generic;
using UnityEngine;

/// <summary>Manages active status effects on an entity. Effects tick on the host
/// (authoritative). Re-applying the same effect refreshes its duration instead of
/// stacking. Slow effects apply a persistent speed multiplier while active.</summary>
public class StatusEffectModule : EntityModule
{
    private readonly List<ActiveEffect> _effects = new();
    private float _slowMultiplier = 1f;

    private class ActiveEffect
    {
        public StatusEffect Definition;
        public float Remaining;
        public float TickTimer;
    }

    public override void Update()
    {
        if (!Helper.IsHost()) return;
        if (EntityMachine.Info == null || EntityMachine.Info.Destroyed) return;

        float dt = Helper.GetDeltaTime();

        for (int i = _effects.Count - 1; i >= 0; i--)
        {
            ActiveEffect effect = _effects[i];
            effect.Remaining -= dt;
            if (effect.Remaining <= 0f)
            {
                _effects.RemoveAt(i);
                continue;
            }

            if (effect.Definition.Type == EffectType.Slow) continue;

            effect.TickTimer += dt;
            if (effect.TickTimer < effect.Definition.TickInterval) continue;
            effect.TickTimer = 0f;

            ApplyTick(effect.Definition);
        }

        UpdateSlow();
    }

    /// <summary>Apply a status effect, refreshing the duration if it is already active.</summary>
    public void Apply(StatusEffect definition)
    {
        if (definition == null) return;

        foreach (ActiveEffect effect in _effects)
        {
            if (effect.Definition.EffectID == definition.EffectID)
            {
                effect.Definition = definition;
                effect.Remaining = definition.Duration;
                effect.TickTimer = 0f;
                return;
            }
        }

        _effects.Add(new ActiveEffect
        {
            Definition = definition,
            Remaining = definition.Duration,
        });
    }

    /// <summary>Remove an active effect by its ID.</summary>
    public void Remove(ID effectID)
    {
        for (int i = _effects.Count - 1; i >= 0; i--)
        {
            if (_effects[i].Definition.EffectID == effectID)
                _effects.RemoveAt(i);
        }
    }

    public bool Has(ID effectID)
    {
        foreach (ActiveEffect effect in _effects)
            if (effect.Definition.EffectID == effectID)
                return true;
        return false;
    }

    private void ApplyTick(StatusEffect definition)
    {
        Info info = EntityMachine.Info;

        switch (definition.Type)
        {
            case EffectType.Heal:
                if (info is DynamicInfo dynamicInfo)
                    dynamicInfo.Health = Mathf.Min(dynamicInfo.HealthMax, dynamicInfo.Health + definition.AmountPerTick);
                else if (info is StructureInfo structure)
                    structure.Health += definition.AmountPerTick;
                break;

            case EffectType.Damage:
                if (info is DynamicInfo damageInfo)
                    damageInfo.Health -= definition.AmountPerTick;
                else if (info is StructureInfo damageStructure)
                    damageStructure.Health -= definition.AmountPerTick;
                break;
        }
    }

    private void UpdateSlow()
    {
        float multiplier = 1f;
        foreach (ActiveEffect effect in _effects)
        {
            if (effect.Definition.Type == EffectType.Slow)
                multiplier = Mathf.Min(multiplier, 1f - effect.Definition.SlowAmount);
        }

        if (Mathf.Approximately(multiplier, _slowMultiplier)) return;

        if (EntityMachine.Info is DynamicInfo dynamicInfo)
            dynamicInfo.SpeedModifier = dynamicInfo.SpeedModifier / _slowMultiplier * multiplier;

        _slowMultiplier = multiplier;
    }
}
