using UnityEngine;
 
public class SwingProjectileInfo : ProjectileInfo
{ 
    public SwingProjectileInfo() { Class = ProjectileClass.Melee; }

    public override void AI(Projectile projectile)
    { 
        Vector3 direction = (projectile.Destination - projectile.transform.position).normalized;
        Vector3 center = projectile.transform.position + direction * (Radius * 0.5f);
        int hitCount = Physics.OverlapSphereNonAlloc(center, Radius / 2, HitBuffer, Game.MaskEntity);
        int landed = 0;
        Info info;
        for (int i = 0; i < hitCount; i++)
        {
            IActionPrimary target = HitBuffer[i].GetComponent<IActionPrimary>();
            if (target == null || projectile.SourceInfo.Machine == (Machine)target) continue;
            info = ((Machine)target).GetModule<Info>();
            info.OnHitInternal(projectile);
            if (info == projectile.SourceInfo.Target)
                projectile.SourceInfo.Target.AbstractHit(projectile.SourceInfo);
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