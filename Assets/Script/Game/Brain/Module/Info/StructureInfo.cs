using System;
using UnityEngine;
using UnityEngine.Serialization;

public enum OperationType { Mining, Building, Cutting, None }

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
    /// <summary>Whether breaking this structure leaves a charred rubble pile. Off for
    /// natural resources (trees, ores) that already yield their loot directly.</summary>
    public bool SpawnsRubble = true;
    /// <summary>Whether this structure's glow light is lit (persists through save/load).</summary>
    public bool GlowOn;
    /// <summary>Item ID of the key needed to unlock this structure. ID.Null means no key required.</summary>
    public ID KeyId = ID.Null;
    [NonSerialized] public SpriteRenderer SpriteRenderer; 

    // Unified breaking rule for any attacker: hostiles bash any structure outright;
    // everyone else needs a matching tool with enough Breaking.
    private bool CanBreak(MobInfo attacker)
    {
        if (attacker.HitboxType == HitboxType.Enemy) return true;
        if (attacker.targetHitboxType == HitboxType.Player || attacker.Equipment == null) return false;
        ProjectileInfo tool = attacker.Equipment.Info.ProjectileInfo;
        return tool.OperationType == operationType && tool.Breaking >= threshold;
    }

    public override bool OnHitInternal(Projectile projectile)
    {
        MobInfo attacker = projectile.SourceInfo;
        if (!CanBreak(attacker)) return false;
        // User-controlled players acquire the target from hitting a structure; AI
        // allies (controllerId == -1) keep the target their brain assigned.
        if (attacker is not PlayerInfo || attacker.controllerId != -1)
            attacker.AcquireTarget(this);
        return true;
    }

    public override void AbstractHit(Projectile projectile)
    {
        MobInfo attacker = projectile.SourceInfo;
        if (!CanBreak(attacker)) return;
        // Hostiles bash with their own attack damage; tool-users break with Breaking.
        int damage = attacker.HitboxType == HitboxType.Enemy
            ? projectile.Info.GetDamage()
            : attacker.Equipment.Info.ProjectileInfo.Breaking;
        Damage(damage, attacker);
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
            OperationType.Building => "Building",
            OperationType.Cutting => "Cutting",
            _ => "Destroying",
        };
        return $"{action}: {Helper.ToDisplayName(id)} | HP {Health}";
    }
}