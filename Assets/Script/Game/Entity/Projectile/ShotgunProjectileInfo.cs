using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>A shotgun blast. On spawn it looses a fan of pellet projectiles with
/// slight horizontal spread, then removes itself. The pellets are plain
/// RangedProjectileInfo; per-pellet state lives on each projectile component, so
/// this shared info instance is safe to fire repeatedly.</summary>
public class ShotgunProjectileInfo : RangedProjectileInfo
{
    /// <summary>How many pellets each shot fires.</summary>
    public int Pellets = 6;
    /// <summary>Maximum horizontal spread in degrees (each side of the aim).</summary>
    public float SpreadAngle = 10f;
    /// <summary>Per-pellet projectile behavior.</summary>
    public RangedProjectileInfo Pellet;

    public override void OnSpawn(Projectile projectile)
    {
        Vector3 origin = projectile.transform.position;
        Vector3 baseDir = projectile.Destination - origin;
        baseDir.y = 0;
        baseDir.Normalize();
        if (baseDir.sqrMagnitude < 0.0001f) baseDir = Vector3.forward;

        for (int i = 0; i < Pellets; i++)
        {
            Vector3 dir = Quaternion.Euler(0, Random.Range(-SpreadAngle, SpreadAngle), 0) * baseDir;
            Projectile.Spawn(origin, origin + dir * 10f, Pellet, projectile.TargetHitBoxType, projectile.SourceInfo);
        }
        projectile.Delete();
    }
}
