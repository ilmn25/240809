using UnityEngine;

public class RangedProjectileInfo : ProjectileInfo
{
    public int LifeSpan; 
    public int Penetration; // ignored for now
    public bool Self = true;

    public RangedProjectileInfo(string sprite, float damage, float knockback, float critChance, int lifeSpan, float speed, float radius, int penetration, bool self = false)
    {
        Damage = damage;
        Knockback = knockback;
        CritChance = critChance;
        LifeSpan = lifeSpan;
        Speed = speed;
        Radius = radius;
        Penetration = penetration;
        Self = self;
        Sprite = sprite;
    }

    public override void AI(Projectile projectile)
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
                projectile.Delete();
                break;  
            } 
            
            IHitBox target = HitBuffer[i].GetComponent<IHitBox>();
            if (target == null) continue;

            if (((Machine)target).GetModule<Info>().OnHitInternal(projectile))
            {
                projectile.Delete();
                break;
            } 
        }
    }
}