using System;
using UnityEngine;
using UnityEngine.Serialization;

public enum OperationType { Mining, Building, Breaking, None }
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
    public override void Initialize()
    {
        SpriteRenderer = Machine.transform.Find("sprite").GetComponent<SpriteRenderer>();
    }

    public override bool OnHitInternal(Projectile projectile)
    {
        projectile.SourceInfo.Target = this;
        projectile.SourceInfo.ActionType = IActionType.Hit;
        // return HandleHealth(projectile.SourceInfo); 
        return true;
    }

    public override void AbstractHit(MobInfo info)
    {
        if (info.Equipment.ProjectileInfo.OperationType != operationType || 
            info.Equipment.ProjectileInfo.Breaking < threshold || 
            info.TargetHitboxType != HitboxType.Passive) return;
        
        Health -= info.Equipment.ProjectileInfo.Breaking;
        if (Health <= 0)
        { 
            Audio.PlaySFX(SfxDestroy);  
            if (Loot != ID.Null) global::Loot.Gettable(Loot).Spawn(position); 
            OnDestroy(info);
            Destroy();
        }
        else
        {
            Audio.PlaySFX(SfxHit); 
            OnHit(info); 
        } 
    }
    public virtual void OnHit(MobInfo info) { }
    public virtual void OnDestroy(MobInfo info) { }
}