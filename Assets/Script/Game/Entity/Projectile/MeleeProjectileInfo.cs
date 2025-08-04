using UnityEngine;

public class MeleeProjectileInfo : ProjectileInfo
{ 
    public float Speed;
    public float Radius;

    protected MeleeProjectileInfo()
    {
        CLass = ProjectileClass.Melee;
    }
}
public class SwingProjectileInfo : MeleeProjectileInfo
{
    public SwingProjectileInfo(float damage, float knockback, float critChance, float speed, float radius)
    {
        Damage = damage;
        Knockback = knockback;
        CritChance = critChance;
        Speed = speed;
        Radius = radius;
    }

    public override void AI(Projectile projectile)
    {  
        Collider[] hitColliders = Physics.OverlapBox(projectile.transform.position, Vector3.one * Radius, Quaternion.identity, Game.MaskEntity);
        IHitBox target;
        foreach (Collider collider in hitColliders)
        {
            target = collider.gameObject.GetComponent<IHitBox>();
            if (target == null) continue;
            target.OnHit(projectile); 
        }
        projectile.Delete();
    }
}