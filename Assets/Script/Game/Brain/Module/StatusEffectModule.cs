using System.Collections.Generic;
using UnityEngine;

/// <summary>Manages active status effects on an entity. Effects tick on the host
/// (authoritative) and each one can drive a looping visual anchored to the entity,
/// so fire follows whatever is burning. Re-applying an effect refreshes its duration
/// rather than stacking. Speed effects are pushed onto DynamicInfo.StatusSpeedMultiplier
/// so they survive AI states rewriting SpeedModifier.</summary>
public class StatusEffectModule : EntityModule
{
    private readonly List<ActiveEffect> _effects = new();
    private float _speedMultiplier = 1f;

    private class ActiveEffect
    {
        public StatusEffect Definition;
        public float Remaining;
        public float TickTimer;
        public float VisualTimer;
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
                RemoveAt(i);
                continue;
            }

            UpdateVisual(effect, dt);

            if (!effect.Definition.Ticks) continue;

            effect.TickTimer += dt;
            if (effect.TickTimer < effect.Definition.TickInterval) continue;
            effect.TickTimer = 0f;

            ApplyTick(effect.Definition);
        }

        UpdateSpeed();
    }

    /// <summary>Emit the effect's looping visual, anchored to this entity so it
    /// tracks them instead of being left behind.</summary>
    private void UpdateVisual(ActiveEffect effect, float dt)
    {
        StatusEffect definition = effect.Definition;
        if (!definition.HasVisual) return;

        effect.VisualTimer += dt;
        if (effect.VisualTimer < Mathf.Max(0.05f, definition.VisualInterval)) return;

        effect.VisualTimer = 0f;
        Particle.CreateAttached(EntityMachine, definition.VisualOffset, definition.Visual, false);
    }

    /// <summary>Apply a status effect, refreshing the duration if it is already active.</summary>
    public void Apply(StatusEffect definition)
    {
        if (definition == null) return;

        foreach (ActiveEffect effect in _effects)
        {
            if (effect.Definition.EffectID != definition.EffectID) continue;
            effect.Definition = definition;
            effect.Remaining = definition.Duration;
            return;
        }

        ActiveEffect newEffect = new ActiveEffect
        {
            Definition = definition,
            Remaining = definition.Duration,
            // Emit on the next update so the effect is visible immediately.
            VisualTimer = definition.VisualInterval,
        };
        if (definition.Type == EffectType.MaxHealthPenalty)
            ApplyMaxHealthPenalty(newEffect);
        _effects.Add(newEffect);
    }

    /// <summary>Remove an active effect by its ID.</summary>
    public void Remove(ID effectID)
    {
        for (int i = _effects.Count - 1; i >= 0; i--)
            if (_effects[i].Definition.EffectID == effectID)
                RemoveAt(i);
    }

    /// <summary>Drop all effects (e.g. on death/respawn), restoring any penalties.</summary>
    public void RemoveAll()
    {
        for (int i = _effects.Count - 1; i >= 0; i--)
            RemoveAt(i);
    }

    private void RemoveAt(int index)
    {
        RestoreMaxHealthPenalty(_effects[index]);
        _effects.RemoveAt(index);
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
    public string ActiveEffectsText(bool showTime = false)
    {
        if (_effects.Count == 0) return "";

        var names = new List<string>(_effects.Count);
        foreach (ActiveEffect effect in _effects)
        {
            string name = DisplayName(effect.Definition);
            names.Add(showTime ? $"{name} ({Mathf.CeilToInt(effect.Remaining)}s)" : name);
        }
        return string.Join(", ", names);
    }

    private static string DisplayName(StatusEffect definition)
    {
        if (!string.IsNullOrEmpty(definition.Name)) return definition.Name;
        return Helper.ToDisplayName(definition.EffectID);
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

    /// <summary>Combine every slow/haste into one multiplier and push it onto the
    /// entity's status speed slot. Keeping it separate from SpeedModifier means an
    /// AI state rewriting SpeedModifier (swing, consume, shoot) can't erase it.</summary>
    private void UpdateSpeed()
    {
        float multiplier = 1f;
        foreach (ActiveEffect effect in _effects)
        {
            switch (effect.Definition.Type)
            {
                case EffectType.Slow:
                    multiplier *= 1f - Mathf.Clamp01(effect.Definition.SpeedAmount);
                    break;
                case EffectType.Haste:
                    multiplier *= 1f + Mathf.Max(0f, effect.Definition.SpeedAmount);
                    break;
            }
        }

        multiplier = Mathf.Max(0.1f, multiplier);
        if (Mathf.Approximately(multiplier, _speedMultiplier)) return;

        if (EntityMachine.Info is DynamicInfo dynamicInfo)
            dynamicInfo.StatusSpeedMultiplier = multiplier;

        _speedMultiplier = multiplier;
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
