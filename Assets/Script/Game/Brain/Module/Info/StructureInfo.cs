using System;
using UnityEngine;
using UnityEngine.Serialization;

public enum OperationType { Mining, Dismantling, Cutting, None }

[System.Serializable]
public class SpriteStructureInfo : StructureInfo
{
    public override void Initialize()
    {
        SpriteRenderer = Machine.transform.Find("Sprite").GetComponent<SpriteRenderer>();
    }
}
[System.Serializable]
public class StructureInfo : Info
{
    public float Health; 
    public int threshold = 1;
    public SfxID SfxHit;
    public SfxID SfxDestroy;
    public ID Loot;
    public OperationType operationType;
    /// <summary>Whether a mallet can lift this structure back into the player's hands.
    /// Player-built structures (anything with a craft recipe) and furniture qualify;
    /// natural and world-generated structures do not.</summary>
    protected virtual bool IsDismantlable =>
        Item.GetItem(id) is { Type: ItemType.Structure } && ItemRecipe.GetRecipe(id) != null;
    /// <summary>Structures currently holding something (a loaded chest, a cooking pot,
    /// a planted planter) refuse to come apart until emptied.</summary>
    protected virtual bool CanDismantleNow => true;
    /// <summary>Shown to the local player when <see cref="CanDismantleNow"/> is false.</summary>
    protected virtual string DismantleBlockedMessage => "I should empty it first.";
    /// <summary>Whether breaking this structure leaves a charred rubble pile. Off for
    /// natural resources (trees, ores) that already yield their loot directly.</summary>
    public bool SpawnsRubble = true;
    /// <summary>Whether this structure's glow light is lit (persists through save/load).</summary>
    public bool GlowOn;
    /// <summary>Item ID of the key needed to unlock this structure. ID.Null means no key required.</summary>
    public ID KeyId = ID.Null;
    [NonSerialized] public SpriteRenderer SpriteRenderer; 

    protected Vector3 OutputOffset()
    {
        return new Vector3(
            UnityEngine.Random.value > 0.5f ? 0.65f : -0.65f,
            1.8f,
            UnityEngine.Random.value > 0.5f ? 0.65f : -0.65f);
    }

    // Unified breaking rule for any attacker: hostiles bash any structure outright;
    // everyone else needs a matching tool with enough Breaking.
    private bool CanBreak(MobInfo attacker)
    {
        if (attacker.HitboxType == HitboxType.Enemy) return true;
        if (attacker.IsNPC || attacker.Equipment == null) return false;
        ProjectileInfo tool = attacker.Equipment.Info.ProjectileInfo;
        return tool.OperationType == operationType && tool.Breaking >= threshold;
    }

    /// <summary>True when the attacker is swinging a dismantling tool (a mallet).
    /// Enemies never dismantle.</summary>
    protected static bool IsDismantlingHit(MobInfo attacker)
    {
        if (attacker == null || attacker.HitboxType == HitboxType.Enemy) return false;
        return attacker.Equipment?.Info?.ProjectileInfo?.OperationType == OperationType.Dismantling;
    }

    private bool CanDismantle(MobInfo attacker) =>
        IsDismantlingHit(attacker) && IsDismantlable && CanDismantleNow;

    private static float _lastDismantleWarnTime = -999f;

    private void WarnCannotDismantle(MobInfo attacker)
    {
        if (attacker != Main.PlayerInfo) return;
        if (Time.time - _lastDismantleWarnTime < 2f) return;
        _lastDismantleWarnTime = Time.time;
        Dialogue.ShowEvent(DismantleBlockedMessage);
    }

    private static float _lastToolWarnTime = -999f;

    // Tell the local player when their tool is too weak for this structure, so a
    // swing with an under-powered tool gives clear feedback instead of doing nothing.
    private void WarnToolTooWeak(MobInfo attacker)
    {
        if (attacker != Main.PlayerInfo || attacker.Equipment?.Info?.ProjectileInfo == null) return;
        ProjectileInfo tool = attacker.Equipment.Info.ProjectileInfo;
        if (tool.OperationType != operationType || tool.Breaking >= threshold) return;
        if (Time.time - _lastToolWarnTime < 2f) return;
        _lastToolWarnTime = Time.time;
        Dialogue.ShowEvent("I need a better tool...");
    }

    public override bool OnHitInternal(Projectile projectile)
    {
        MobInfo attacker = projectile.SourceInfo;
        if (IsDismantlingHit(attacker) && IsDismantlable)
        {
            if (!CanDismantleNow)
            {
                WarnCannotDismantle(attacker);
                return false;
            }
            // PlayerInfo ignores this for AI-controlled players; mobs keep their assigned target.
            if (attacker is PlayerInfo)
                attacker.AcquireTarget(this);
            return true;
        }
        if (!CanBreak(attacker))
        {
            WarnToolTooWeak(attacker);
            return false;
        }
        // PlayerInfo ignores this for AI-controlled players; mobs keep their assigned target.
        if (attacker is PlayerInfo)
            attacker.AcquireTarget(this);
        return true;
    }

    public override void AbstractHit(Projectile projectile)
    {
        MobInfo attacker = projectile.SourceInfo;
        if (CanDismantle(attacker))
        {
            Dismantle(attacker);
            return;
        }
        if (!CanBreak(attacker)) return;
        // Hostiles bash with their own attack damage; tool-users break with Breaking.
        int damage = attacker.HitboxType == HitboxType.Enemy
            ? projectile.Info.GetDamage()
            : attacker.Equipment.Info.ProjectileInfo.Breaking;
        Damage(damage, attacker);
    }

    /// <summary>Take a player-placed structure apart intact: spill whatever it holds,
    /// drop the structure item back out, and clear it without damaging it or leaving
    /// rubble.</summary>
    private void Dismantle(MobInfo attacker)
    {
        if (Destroyed) return;
        Audio.PlaySFX(SfxDestroy);
        // Teardown side effects (container contents, planter crops) run before the
        // machine leaves the world so their positions are still valid.
        OnDestroy(attacker);
        Entity.SpawnItem(id, position);
        RemoveFromChunk();
        // Clear the attacker's target so the swing animation isn't reset by
        // re-targeting this now-removed structure.
        if (attacker != null && attacker.Target == this)
            attacker.CancelTarget();
        Destroy();
    }

    // Shared damage path for any attacker (tool breaking or enemy bashing).
    private void Damage(int damage, MobInfo info)
    {
        if (Destroyed) return;
        Health -= damage;
        Tutorial.OnTreeHit(this, info);
        if (Health <= 0)
        { 
            Audio.PlaySFX(SfxDestroy);  
            if (Loot != ID.Null)
                global::Loot.Gettable(Loot).Spawn(position);
            if (SpawnsRubble)
                Entity.Spawn(ID.Rubble, Vector3Int.FloorToInt(position));
            OnDestroy(info);
            // Clear the attacker's target so the swing animation isn't reset by
            // re-targeting this now-destroyed structure (null attacker = no target).
            if (info != null && info.Target == this)
                info.CancelTarget();
            Destroy();
        }
        else
        {
            Audio.PlaySFX(SfxHit);
            Particle.Create(position, Particles.HitDust, false);
            
            if (Loot != ID.Null && Health < 25 && UnityEngine.Random.Range(0, 14) == 0)
            {
                Vector3 offset = new Vector3(
                    UnityEngine.Random.value > 0.5f ? 0.65f : -0.65f,
                    1.8f,
                    UnityEngine.Random.value > 0.5f ? 0.65f : -0.65f);
                global::Loot.Gettable(Loot).SpawnOneRandom(position + offset);
            }
            
            OnHit(info); 
        } 
    }

    public virtual void OnHit(MobInfo info) { }
    public virtual void OnDestroy(MobInfo info) { }

    /// <summary>Environmental damage (e.g. lightning) with no attacker, so no
    /// target acquisition. Breaks and drops loot like normal structure damage.</summary>
    public void ApplyEnvironmentalDamage(int damage) => Damage(damage, null);

    /// <summary>Drop a placed structure from its chunk so it doesn't persist or leave
    /// a stale map marker (used when a structure is removed, e.g. broken or picked up).</summary>
    protected void RemoveFromChunk()
    {
        Vector3Int chunkCoord = World.GetChunkCoordinate(Machine.transform.position);
        World.Inst[chunkCoord].StaticEntity.Remove(this);
        if (World.Inst.Map != null)
        {
            World.Inst.Map.Dirty = true;
            World.Inst.Map.ResetMarkers();
        }
    }

    public override string ToString()
    {
        string action = operationType switch
        {
            OperationType.Mining => "Mining",
            OperationType.Dismantling => "Dismantling",
            OperationType.Cutting => "Cutting",
            _ => "Destroying",
        };
        return $"{action}: {Helper.ToDisplayName(id)} | HP {Health}";
    }
}