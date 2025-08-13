using UnityEngine;

public class RangedProjectileInfo : ProjectileInfo
{
    public int LifeSpan; 
    public int Penetration; // ignored for now
    public bool Self = true;
    public bool Stuck = false;
    public int LodgeTime = 0;

    public override void AI(Projectile projectile)
    {
        if (projectile.LodgeTime == 0)
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
                if (Utility.IsInLayerMask(HitBuffer[i].gameObject, Game.MaskMap))
                {
                    Audio.PlaySFX("dig_stone");
                    projectile.LodgeTime = 1;
                    break;  
                } 
            
                IHitBox target = HitBuffer[i].GetComponent<IHitBox>();
                if (target == null) continue;
            
                if (((Machine)target).GetModule<Info>().OnHitInternal(projectile))
                {
                    projectile.Target = (Machine)target;
                    projectile.LodgeTime = 1;
                    break;
                } 
            } 
        }
        else
        {
            if (projectile.Target) projectile.transform.position = projectile.Target.transform.position + Vector3.up * 0.35f;
            if (projectile.LodgeTime > LodgeTime || (projectile.Target && !projectile.Target.isActiveAndEnabled))
            {
                projectile.Delete();
            }
            projectile.LodgeTime++;
        }
    }
}