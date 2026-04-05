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
    [NonSerialized] public SpriteRenderer SpriteRenderer; 

    public override bool OnHitInternal(Projectile projectile)
    {
        if (projectile.SourceInfo.Equipment == null ||
            projectile.SourceInfo.targetHitboxType == HitboxType.Player ||
            projectile.SourceInfo.Equipment.Info.ProjectileInfo.OperationType != operationType ||
            projectile.SourceInfo.Equipment.Info.ProjectileInfo.Breaking < threshold)
        { 
            return false;
        }  
        projectile.SourceInfo.Target = this;
        projectile.SourceInfo.ActionType = IActionType.Hit;;
        return true;
        // if (!PlayerTask.Pending.Contains(this)) PlayerTask.Pending.Add(this) 
    }

    public override void AbstractHit(MobInfo info)
    {
        if ( info.targetHitboxType == HitboxType.Player ||
             info.Equipment == null ||
             info.Equipment.Info.ProjectileInfo.OperationType != operationType || 
             info.Equipment.Info.ProjectileInfo.Breaking < threshold) return;
        
        Health -= info.Equipment.Info.ProjectileInfo.Breaking;
        if (Health <= 0)
        { 
            Audio.PlaySFX(SfxDestroy);  
            if (Loot != ID.Null)
                global::Loot.Gettable(Loot).Spawn(position);
            OnDestroy(info);
            PlayerTask.Pending.Remove(this);
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

    public override string ToString()
    {
        string action = operationType switch
        {
            OperationType.Mining => "Mining",
            OperationType.Building => "Building",
            OperationType.Cutting => "Cutting",
            _ => "Destroying",
        };
        return $"{action}: {FormatId(id)} | HP {Health}";
    }
}