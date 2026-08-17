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
    /// <summary>Whether this structure's glow light is lit (persists through save/load).</summary>
    public bool GlowOn;
    /// <summary>Melee damage an enemy deals to this structure per hit (doors, barricades, ...).</summary>
    public int EnemyBashDamage = 8;
    /// <summary>If true, enemies will proactively bash this structure when it blocks their path.</summary>
    public bool EnemyBreakable;
    [NonSerialized] public SpriteRenderer SpriteRenderer; 

    public override bool OnHitInternal(Projectile projectile)
    {
        // Enemies can bash any structure down with their melee — no tool required.
        // (Actual damage is applied in AbstractHit, which contact projectiles now
        // also route through — this just validates the hit.)
        if (projectile.SourceInfo.HitboxType == HitboxType.Enemy)
            return true;
        if (projectile.SourceInfo.Equipment == null ||
            projectile.SourceInfo.targetHitboxType == HitboxType.Player ||
            projectile.SourceInfo.Equipment.Info.ProjectileInfo.OperationType != operationType ||
            projectile.SourceInfo.Equipment.Info.ProjectileInfo.Breaking < threshold)
        { 
            return false;
        }  
        // User-controlled players acquire the target from hitting a structure; AI
        // allies (controllerId == -1) keep the target their brain assigned.
        if (projectile.SourceInfo is not PlayerInfo || projectile.SourceInfo.controllerId != -1)
            projectile.SourceInfo.AcquireTarget(this);
        return true;
    }

    public override void AbstractHit(MobInfo info)
    {
        // Enemies bash the structure down with their melee — no tool requirement.
        if (info.HitboxType == HitboxType.Enemy)
        {
            Damage(EnemyBashDamage, info);
            return;
        }
        if ( info.targetHitboxType == HitboxType.Player ||
             info.Equipment == null ||
             info.Equipment.Info.ProjectileInfo.OperationType != operationType || 
             info.Equipment.Info.ProjectileInfo.Breaking < threshold) return;
        
        Damage(info.Equipment.Info.ProjectileInfo.Breaking, info);
    }

    // Shared damage path for players (tool Breaking) and enemies bashing via AbstractHit.
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