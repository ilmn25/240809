using UnityEngine;

public class MeleeProjectileInfo : ProjectileInfo
{ 
    public float Speed;
    public float Range;

    protected MeleeProjectileInfo()
    {
        CLass = ProjectileClass.Melee;
    }
}
public class SwingProjectileInfo : MeleeProjectileInfo
{
    public SwingProjectileInfo(float damage, float knockback, float critChance, float speed, float range)
    {
        Damage = damage;
        Knockback = knockback;
        CritChance = critChance;
        Speed = speed;
        Range = range;
    }

    public override void AI(Projectile projectile)
    {
        Vector3 direction = (projectile.Destination - projectile.transform.position).normalized;
        Vector3 boxCenter = projectile.transform.position + direction * (Range * 0.5f);
        Vector3 boxSize = Vector3.one * (Range * 0.5f);

        Collider[] hitColliders = Physics.OverlapBox(boxCenter, boxSize, Quaternion.identity, Game.MaskEntity);
        foreach (Collider collider in hitColliders)
        {
            IHitBox target = collider.gameObject.GetComponent<IHitBox>();
            if (target == null) continue;

            ((Machine)target).GetModule<StatusModule>().OnHitInternal(projectile);
        }

        projectile.Delete();
    }

}