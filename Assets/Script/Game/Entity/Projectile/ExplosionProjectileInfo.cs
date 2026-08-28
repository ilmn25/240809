using UnityEngine;

/// <summary>A stationary area-of-effect blast. On spawn it damages every entity
/// within Radius (via OnHitInternal, so it respects target hitbox rules), then
/// removes itself.</summary>
public class ExplosionProjectileInfo : ProjectileInfo
{
    /// <summary>Whether the blast spares its source (used for a thrown grenade so
    /// a point-blank throw can't blow the thrower up).</summary>
    public bool SkipSource;

    public override void OnSpawn(Projectile projectile)
    {
        // Local buffer (not the shared static HitBuffer): explosions can trigger
        // nested explosions (landmine/grenade chain reactions), and a nested blast
        // must not clobber this loop's results mid-iteration.
        Collider[] buffer = new Collider[50];
        int hitCount = Physics.OverlapSphereNonAlloc(projectile.transform.position, Radius, buffer);
        for (int i = 0; i < hitCount; i++)
        {
            if (SkipSource && projectile.SourceInfo != null && projectile.SourceInfo.Machine != null &&
                projectile.SourceInfo.Machine.gameObject == buffer[i].gameObject)
                continue;

            IActionPrimary target = buffer[i].GetComponent<IActionPrimary>();
            if (target == null) continue;
            Info info = ((Machine)target).GetModule<Info>();
            if (info is StructureInfo structure)
                structure.ApplyEnvironmentalDamage(projectile.Info.GetDamage());
            else
                info.OnHitInternal(projectile);
        }
        projectile.Delete();
    }
}