using UnityEngine;

public class RangedProjectileInfo : ProjectileInfo
{
    public int LifeSpan; 
    public int Penetration;
    public float Gravity;

    protected virtual void OnHitTarget(Projectile projectile, Machine target) { }

    public override void OnSpawn(Projectile projectile)
    {
        projectile.Velocity = Gravity == 0f ? projectile.Direction * Speed : LaunchVelocity(projectile);
    }

    private Vector3 LaunchVelocity(Projectile projectile)
    {
        Vector3 delta = projectile.Destination - projectile.transform.position;
        Vector3 horizontal = new Vector3(delta.x, 0f, delta.z);
        float distance = horizontal.magnitude;
        if (distance < 0.5f) return projectile.Direction * Speed;

        float g = Mathf.Abs(Gravity);
        float speedSqr = Speed * Speed;
        float discriminant = speedSqr * speedSqr - g * (g * distance * distance + 2f * delta.y * speedSqr);
        float angle = Mathf.Atan(discriminant >= 0f
            ? (speedSqr - Mathf.Sqrt(discriminant)) / (g * distance)
            : 1f);

        return (horizontal / distance * Mathf.Cos(angle) + Vector3.up * Mathf.Sin(angle)) * Speed;
    }

    private void Land(Projectile projectile, Vector3 position)
    {
        if (Ammo != ID.Null) Entity.SpawnItem(Ammo, position);
        projectile.Delete();
    }

    public override void AI(Projectile projectile)
    {
        if (projectile.LifeSpan > LifeSpan)
        {
            projectile.Delete();
            return;
        }

        projectile.Velocity.y += Gravity * Time.deltaTime;
        projectile.transform.position += projectile.Velocity * Time.deltaTime;

        int hitCount = Physics.OverlapSphereNonAlloc(projectile.transform.position, Radius, HitBuffer);
        for (int i = 0; i < hitCount; i++)
        {
            if (Helper.IsInLayerMask(HitBuffer[i].gameObject, Main.MaskMap))
            {
                Audio.PlaySFX(SfxID.HitStone);
                Particle.Create(projectile.transform.position, Particles.Spark, false);
                Land(projectile, projectile.transform.position - projectile.Direction.normalized * 0.5f);
                return;
            }

            IActionPrimary target = HitBuffer[i].GetComponent<IActionPrimaryAttack>();
            if (target == null || projectile.SourceInfo.Machine == (Machine)target) continue;

            Info targetInfo = ((Machine)target).GetModule<Info>();
            if (targetInfo.OnHitInternal(projectile))
            {
                Particle.Create(projectile.transform.position, Particles.Spark, false);
                OnHitTarget(projectile, (Machine)target);
                Land(projectile, projectile.transform.position);
                return;
            }
        }
    }
}  