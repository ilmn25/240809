using UnityEngine;

public class RangedProjectileInfo : ProjectileInfo
{
    public int LifeSpan; 
    public int Penetration; // ignored for now
    public bool Self = true;
    public bool Stuck = false;
    public bool Lodge = false;
    public bool PickUp = false;

    public override void AI(Projectile projectile)
    {
        if (!projectile.Target)
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
                if (Helper.IsInLayerMask(HitBuffer[i].gameObject, Game.MaskMap))
                {
                    Audio.PlaySFX("dig_stone");
                    if (PickUp) Entity.SpawnItem(Ammo, projectile.transform.position - projectile.Direction.normalized * 0.5f);
                    projectile.Delete();
                    break;  
                } 
            
                IActionPrimary target = HitBuffer[i].GetComponent<IActionPrimaryAttack>();
                if (target == null || projectile.SourceInfo.Machine == (Machine)target) continue;
                 
                if (((Machine)target).GetModule<Info>().OnHitInternal(projectile))
                {
                    if (Lodge)
                    {
                        projectile.Target = ((MobMachine)target).Info.SpriteToolTrack ?? projectile.Target.transform;;
                        projectile.RelativeRotation = Quaternion.Inverse(projectile.Target.rotation) * projectile.transform.rotation;
                    }
                    else
                    {
                        projectile.Delete();
                    }
                    break;
                } 
            } 
        }
        else
        {
            projectile.transform.position = projectile.Target.position;
            projectile.transform.rotation = projectile.Target.rotation * projectile.RelativeRotation;
            
            if (!projectile.Target.gameObject.activeSelf)
            {
                if (PickUp) Entity.SpawnItem(Ammo, projectile.transform.position);
                projectile.Delete();    
            }
        }
    }
}