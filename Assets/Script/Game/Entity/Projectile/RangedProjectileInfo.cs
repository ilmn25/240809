using UnityEngine;

public class RangedProjectileInfo : ProjectileInfo
{
    public int LifeSpan; 
    public int Penetration; // ignored for now
    public bool Stuck = false;
    public bool Lodge = false;
    public bool PickUp = false;
    /// <summary>Chance (0..1) that a throwable shatters where it lands instead of waiting to be
    /// picked up again. 0 keeps every throw recoverable.</summary>
    public float BreakChance;

    /// <summary>Hook called after a successful hit on an entity (before it is
    /// deleted or lodged). Subclasses add extra on-hit effects here.</summary>
    protected virtual void OnHitTarget(Projectile projectile, Machine target) { }

    /// <summary>Leave the thrown item where it landed, unless the throw broke it.</summary>
    private void DropForPickup(Vector3 position)
    {
        if (!PickUp || Ammo == ID.Null) return;
        if (Random.value < BreakChance) return;

        Entity.SpawnItem(Ammo, position);
    }

    public override void AI(Projectile projectile)
    {
        if (projectile.Target == null)
        {
            // Check lifespan
            if (projectile.LifeSpan > LifeSpan)
            { 
                projectile.Delete(); 
                return;
            }
 
            projectile.transform.position += projectile.Direction * (Speed * Time.deltaTime);

            // Check for collision using non-alloc
            int hitCount = Physics.OverlapSphereNonAlloc(projectile.transform.position, Radius, HitBuffer);
            for (int i = 0; i < hitCount; i++)
            {
                if (Helper.IsInLayerMask(HitBuffer[i].gameObject, Main.MaskMap))
                {
                    Audio.PlaySFX(SfxID.HitStone);
                    DropForPickup(projectile.transform.position - projectile.Direction.normalized * 0.5f);
                    projectile.Delete();
                    break;  
                } 
            
                IActionPrimary target = HitBuffer[i].GetComponent<IActionPrimaryAttack>();
                if (target == null || projectile.SourceInfo.Machine == (Machine)target) continue;

                Info targetInfo = ((Machine)target).GetModule<Info>();
                if (targetInfo.OnHitInternal(projectile))
                {
                    OnHitTarget(projectile, (Machine)target);
                    // Only mobs have a hand mount to ride along with; a lodged
                    // throwable that hits a plant or structure just drops there.
                    if (Lodge && targetInfo is DynamicInfo dynamicTarget)
                    {
                        projectile.Target = dynamicTarget;
                        projectile.RelativeRotation = Quaternion.Inverse(projectile.Target.SpriteToolTrack.rotation) * projectile.transform.rotation;
                    }
                    else
                    {
                        DropForPickup(projectile.transform.position);
                        projectile.Delete();
                    }
                    break;
                } 
            } 
        }
        else
        {
            projectile.transform.position = projectile.Target.SpriteToolTrack.position;
            projectile.transform.rotation = projectile.Target.SpriteToolTrack.rotation * projectile.RelativeRotation;
            
            if (projectile.Target.Destroyed)
            {
                DropForPickup(projectile.transform.position);
                projectile.Delete();    
            }
        }
    }
}  