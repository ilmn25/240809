using UnityEngine;

public class StructureInfo : Info
{
    public float Health;
    public string SfxHit;
    public string SfxDestroy;
    public string Loot;
    private BoxCollider _boxCollider;
 
    public override bool OnHitInternal(Projectile projectile)
    {
        if (projectile.Info.Breaking == 0 || projectile.TargetHitBoxType != HitboxType.Passive) return false;
        Health -= projectile.Info.Breaking;
        if (Health <= 0)
        {
            Audio.PlaySFX(SfxDestroy); 
            ((EntityMachine)Machine).Delete();
            global::Loot.Gettable(Loot).Spawn(Machine.transform.position);
            OnHit();
        }
        else
        {
            Audio.PlaySFX(SfxHit); 
            OnDestroy();
        }
        return true;
    }

    public virtual void OnHit() { }
    public virtual void OnDestroy() { }
}


public class ContainerInfo : StructureInfo
{
    public Storage Storage;
    public override void OnDestroy()
    {
        Storage.Explode(Machine.transform.position);
    }
}