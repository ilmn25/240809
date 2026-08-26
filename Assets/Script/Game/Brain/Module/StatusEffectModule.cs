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
        public int MaxHealthReduced;
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
                RestoreMaxHealthPenalty(effect);
                _effects.RemoveAt(i);
                continue;
            }

            if (effect.Definition.Type == EffectType.Slow ||
                effect.Definition.Type == EffectType.MaxHealthPenalty) continue;

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
                return;
            }
        }

        ActiveEffect newEffect = new ActiveEffect
        {
            Definition = definition,
            Remaining = definition.Duration,
        };
        if (definition.Type == EffectType.MaxHealthPenalty)
            ApplyMaxHealthPenalty(newEffect);
        _effects.Add(newEffect);
    }

    /// <summary>Remove an active effect by its ID.</summary>
    public void Remove(ID effectID)
    {
        for (int i = _effects.Count - 1; i >= 0; i--)
        {
            if (_effects[i].Definition.EffectID == effectID)
            {
                RestoreMaxHealthPenalty(_effects[i]);
                _effects.RemoveAt(i);
            }
        }
    }

    public bool Has(ID effectID)
    {
        foreach (ActiveEffect effect in _effects)
            if (effect.Definition.EffectID == effectID)
                return true;
        return false;
    }

    /// <summary>Comma-separated names of the currently active effects, or empty
    /// when none are active. Used for HUD / info-panel display.</summary>
    public string ActiveEffectsText()
    {
        if (_effects.Count == 0) return "";

        var names = new List<string>(_effects.Count);
        foreach (ActiveEffect effect in _effects)
        {
            string name = effect.Definition.Name;
            if (string.IsNullOrEmpty(name))
                name = Helper.ToDisplayName(effect.Definition.EffectID);
            names.Add(name);
        }
        return string.Join(", ", names);
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

    private void ApplyMaxHealthPenalty(ActiveEffect effect)
    {
        int amount = effect.Definition.AmountPerTick;
        if (EntityMachine.Info is PlayerInfo player)
        {
            player.BaseHealthMax = Mathf.Max(1, player.BaseHealthMax - amount);
            player.HealthMax = Mathf.Max(1, player.HealthMax - amount);
            if (player.Health > player.HealthMax) player.Health = player.HealthMax;
            // Refresh the HUD heart bar so the lowered max HP shows immediately.
            if (Main.PlayerInfo == player) GUIBar.Update();
        }
        else if (EntityMachine.Info is DynamicInfo dynamicInfo)
        {
            dynamicInfo.HealthMax = Mathf.Max(1, dynamicInfo.HealthMax - amount);
            if (dynamicInfo.Health > dynamicInfo.HealthMax) dynamicInfo.Health = dynamicInfo.HealthMax;
        }
        effect.MaxHealthReduced = amount;
    }

    private void RestoreMaxHealthPenalty(ActiveEffect effect)
    {
        if (effect.MaxHealthReduced == 0) return;
        int amount = effect.MaxHealthReduced;
        if (EntityMachine.Info is PlayerInfo player)
        {
            player.BaseHealthMax += amount;
            player.HealthMax += amount;
            // Refresh the HUD heart bar so the restored max HP shows immediately.
            if (Main.PlayerInfo == player) GUIBar.Update();
        }
        else if (EntityMachine.Info is DynamicInfo dynamicInfo)
        {
            dynamicInfo.HealthMax += amount;
        }
        effect.MaxHealthReduced = 0;
    }
}
