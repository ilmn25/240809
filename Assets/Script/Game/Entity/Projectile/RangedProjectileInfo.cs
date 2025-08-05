using UnityEngine;

public class RangedProjectileInfo : ProjectileInfo
{
    public int LifeSpan;
    public float Speed;
    public float Radius;  
    public int Penetration; // ignored for now
    public bool Self = true;

    public RangedProjectileInfo(float damage, float knockback, float critChance, int lifeSpan, float speed, float radius, int penetration, bool self = false)
    {
        Damage = damage;
        Knockback = knockback;
        CritChance = critChance;
        LifeSpan = lifeSpan;
        Speed = speed;
        Radius = radius;
        Penetration = penetration;
        Self = self;
        Sprite = Cache.LoadSprite("sprite/bullet");
    }

    // Reusable buffer for collision detection
    private static readonly Collider[] HitBuffer = new Collider[8]; // Adjust size as needed

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

            if (((Machine)target).GetModule<HitboxModule>().OnHitInternal(projectile))
            {
                projectile.Delete();
                break;
            } 
        }
    }
}