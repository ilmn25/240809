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
        if (projectile.Info.OperationType != operationType || 
            projectile.Info.Breaking < threshold || 
            projectile.TargetHitBoxType != HitboxType.Passive) return false;
        
        Health -= projectile.Info.Breaking;
        if (Health <= 0)
        { 
            Audio.PlaySFX(SfxDestroy);  
            if (Loot != ID.Null) global::Loot.Gettable(Loot).Spawn(Machine.transform.position); 
            OnDestroy(projectile);
            ((EntityMachine)Machine).Delete();
        }
        else
        {
            Audio.PlaySFX(SfxHit); 
            OnHit(projectile); 
        }
        return true;
    }
 
    public virtual void OnHit(Projectile projectile) { }
    public virtual void OnDestroy(Projectile projectile) { }
}