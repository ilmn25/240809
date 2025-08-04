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
    }

    // Reusable buffer for collision detection
    private static readonly Collider[] HitBuffer = new Collider[8]; // Adjust size as needed

    public override void AI(Projectile projectile)
    {
        // Check lifespan
        if (projectile.LifeSpan > LifeSpan || Vector3.Distance(projectile.transform.position, projectile.Destination) < Radius)
        {
            projectile.Delete();
            return;
        }

        // Move toward destination
        Vector3 direction = (projectile.Destination - projectile.transform.position).normalized;
        projectile.transform.position += direction * (Speed * Time.deltaTime);

        // Check for collision using non-alloc
        int hitCount = Physics.OverlapSphereNonAlloc(projectile.transform.position, Radius, HitBuffer, Game.MaskEntity);
        for (int i = 0; i < hitCount; i++)
        {
            IHitBox target = HitBuffer[i].GetComponent<IHitBox>();
            if (target == null) continue;

            if (((Machine)target).GetModule<StatusModule>().OnHitInternal(projectile))
            {
                projectile.Delete();
                break;
            } 
        }
    }
}