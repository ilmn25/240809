using UnityEngine;
 
public class SwingProjectileInfo : ProjectileInfo
{ 
    public SwingProjectileInfo() { Class = ProjectileClass.Melee; }

    public override void AI(Projectile projectile)
    {
        Vector3 direction = (projectile.Destination - projectile.transform.position).normalized;
        Vector3 center = projectile.transform.position + direction * (Radius * 0.5f);
        int hitCount = Physics.OverlapSphereNonAlloc(center, Radius / 2, HitBuffer, Game.MaskEntity);

        for (int i = 0; i < hitCount; i++)
        {
            IActionPrimary target = HitBuffer[i].GetComponent<IActionPrimary>();
            if (target == null || projectile.SourceInfo.Machine == (Machine)target) continue;

            if (((Machine)target).GetModule<Info>().OnHitInternal(projectile))
                projectile.Delete();
                
        }

        projectile.Delete();
    }
}

public class ContactDamageProjectileInfo : ProjectileInfo
{ 
    public override void AI(Projectile projectile)
    {
        Collider[] _hitBuffer = new Collider[16];
        int hitCount =
            Physics.OverlapSphereNonAlloc(projectile.transform.position, Radius, _hitBuffer, Game.MaskEntity);

        for (int i = 0; i < hitCount; i++)
        {
            IActionPrimary target = _hitBuffer[i].GetComponent<IActionPrimary>();
            if (target == null || projectile.SourceInfo.Machine == (Machine)target) continue;

            ((Machine)target).GetModule<Info>().OnHitInternal(projectile);
        }

        projectile.Delete();
    }
}