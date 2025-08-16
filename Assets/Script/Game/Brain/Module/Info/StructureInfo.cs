using System;
using UnityEngine;

[System.Serializable]
public class StructureInfo : Info
{
    public float Health;
    public string SfxHit;
    public string SfxDestroy;
    public string Loot;
    [NonSerialized] private BoxCollider _boxCollider;
 
    public override bool OnHitInternal(Projectile projectile)
    {
        if (projectile.Info.Breaking == 0 || projectile.TargetHitBoxType != HitboxType.Passive) return false;
        Health -= projectile.Info.Breaking;
        if (Health <= 0)
        {
            Audio.PlaySFX(SfxDestroy); 
            ((EntityMachine)Machine).Delete();
            global::Loot.Gettable(Loot).Spawn(Machine.transform.position); 
            OnDestroy(projectile);
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