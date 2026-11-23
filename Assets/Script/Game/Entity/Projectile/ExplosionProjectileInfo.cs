using UnityEngine;

public class ExplosionProjectileInfo : ProjectileInfo
{
    public bool SkipSource;
    public float ShakeSpeed = 80f;
    public float ShakeStrength = 0.4f;
    public float ShakeDuration = 0.5f;

    public override void OnSpawn(Projectile projectile)
    {
        Vector3 position = projectile.transform.position;
        Audio.PlaySFX(SfxID.Thunder);
        if (Helper.IsHost())
        {
            Particle.Create(position, Particles.Fire, false);
            Particle.Create(position, Particles.Smoke, false);
            Particle.Create(position, Particles.HitDust, false);
        }
        ScreenShake.Shake(ShakeSpeed, ShakeStrength, ShakeDuration);

        Collider[] buffer = new Collider[50];
        int hitCount = Physics.OverlapSphereNonAlloc(position, Radius, buffer);
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