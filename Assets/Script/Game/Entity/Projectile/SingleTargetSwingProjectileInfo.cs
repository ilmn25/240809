using UnityEngine;

/// <summary>
/// A melee swing that hits only the single closest valid target, instead of
/// every target in the swing radius. Used for precision weapons (rapier) and
/// gathering tools (axe, pickaxe, hammer) so they don't cleave through crowds.
/// </summary>
public class SingleTargetSwingProjectileInfo : ProjectileInfo
{
    public SingleTargetSwingProjectileInfo() { Class = ProjectileClass.Melee; }

    public override void AI(Projectile projectile)
    {
        Vector3 direction = (projectile.Destination - projectile.transform.position).normalized;
        Vector3 center = projectile.transform.position + direction * (Radius * 0.5f);
        int hitCount = Physics.OverlapSphereNonAlloc(center, Radius / 2, HitBuffer, Main.MaskEntity);

        // Find the closest valid target.
        Machine closest = null;
        float closestDistSqr = float.MaxValue;
        for (int i = 0; i < hitCount; i++)
        {
            IActionPrimary target = HitBuffer[i].GetComponent<IActionPrimary>();
            if (target == null || projectile.SourceInfo.Machine == (Machine)target) continue;

            float distSqr = (HitBuffer[i].transform.position - projectile.transform.position).sqrMagnitude;
            if (distSqr < closestDistSqr)
            {
                closestDistSqr = distSqr;
                closest = (Machine)target;
            }
        }

        if (closest != null)
        {
            Info info = closest.GetModule<Info>();
            info.OnHitInternal(projectile);
            if (info == projectile.SourceInfo.Target)
                projectile.SourceInfo.Target.AbstractHit(projectile.SourceInfo);
        }

        projectile.Delete();
    }
}
