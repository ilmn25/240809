using UnityEngine;
 
/// <summary>A melee swing: <see cref="Targets"/> 0 hits everything in the arc, 1 the closest.</summary>
public class SwingProjectileInfo : ProjectileInfo
{ 
    public int Targets;

    public SwingProjectileInfo() { Class = ProjectileClass.Melee; }

    public override void AI(Projectile projectile)
    { 
        Vector3 direction = (projectile.Destination - projectile.transform.position).normalized;
        Vector3 center = projectile.transform.position + direction * (Radius * 0.5f);
        HitSphere(projectile, center, Radius / 2, Targets);
        projectile.Delete();
    }
}

public class ContactDamageProjectileInfo : ProjectileInfo
{ 
    public override void AI(Projectile projectile)
    {
        HitSphere(projectile, projectile.transform.position, Radius);
        projectile.Delete();
    }
}