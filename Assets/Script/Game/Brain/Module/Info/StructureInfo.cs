using System;
using UnityEngine;
using UnityEngine.Serialization;

public enum OperationType { Dig, Build, Break, None }
[System.Serializable]
public class StructureInfo : Info
{
    public float Health; 
    public int threshold = 1;
    public string SfxHit;
    public string SfxDestroy;
    public string Loot;
    public OperationType operationType;
 
    public override bool OnHitInternal(Projectile projectile)
    {
        if (projectile.Info.OperationType != operationType || 
            projectile.Info.Breaking < threshold || 
            projectile.TargetHitBoxType != HitboxType.Passive) return false;
        
        Health -= projectile.Info.Breaking;
        if (Health <= 0)
        { 
            Audio.PlaySFX(SfxDestroy);  
            if (Loot != null) global::Loot.Gettable(Loot).Spawn(Machine.transform.position); 
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