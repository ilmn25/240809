using UnityEngine;

/// <summary>A stationary area-of-effect blast. On spawn it damages every entity
/// within Radius (via OnHitInternal, so it respects target hitbox rules), then
/// removes itself.</summary>
public class ExplosionProjectileInfo : ProjectileInfo
{
    public override void OnSpawn(Projectile projectile)
    {
        int hitCount = Physics.OverlapSphereNonAlloc(projectile.transform.position, Radius, HitBuffer);
        for (int i = 0; i < hitCount; i++)
        {
            IActionPrimary target = HitBuffer[i].GetComponent<IActionPrimary>();
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