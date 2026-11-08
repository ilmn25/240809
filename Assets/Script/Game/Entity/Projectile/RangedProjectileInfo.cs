using UnityEngine;

public class RangedProjectileInfo : ProjectileInfo
{
    public int LifeSpan; 
    public int Penetration; // ignored for now
    public bool Lodge = false;
    /// <summary>Chance (0..1) that a thrown item lands whole and can be picked up again;
    /// the rest of the time the throw destroys it. 0 for shots that are spent on firing.</summary>
    public float PickUpChance;

    /// <summary>Hook called after a successful hit on an entity (before it is
    /// deleted or lodged). Subclasses add extra on-hit effects here.</summary>
    protected virtual void OnHitTarget(Projectile projectile, Machine target) { }

    /// <summary>End the throw where it landed: leave the item behind if it survived the throw.</summary>
    private void Land(Projectile projectile, Vector3 position)
    {
        if (Ammo != ID.Null && Random.value < PickUpChance) Entity.SpawnItem(Ammo, position);
        projectile.Delete();
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
                    Land(projectile, projectile.transform.position - projectile.Direction.normalized * 0.5f);
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
                        Land(projectile, projectile.transform.position);
                    break;
                } 
            } 
        }
        else
        {
            projectile.transform.position = projectile.Target.SpriteToolTrack.position;
            projectile.transform.rotation = projectile.Target.SpriteToolTrack.rotation * projectile.RelativeRotation;
            
            if (projectile.Target.Destroyed)
                Land(projectile, projectile.transform.position);
        }
    }
}  