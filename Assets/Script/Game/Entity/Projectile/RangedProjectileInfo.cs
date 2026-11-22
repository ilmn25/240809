using UnityEngine;

public class RangedProjectileInfo : ProjectileInfo
{
    public int LifeSpan; 
    public int Penetration;

    protected virtual void OnHitTarget(Projectile projectile, Machine target) { }

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

        projectile.transform.position += projectile.Direction * (Speed * Time.deltaTime);

        int hitCount = Physics.OverlapSphereNonAlloc(projectile.transform.position, Radius, HitBuffer);
        for (int i = 0; i < hitCount; i++)
        {
            if (Helper.IsInLayerMask(HitBuffer[i].gameObject, Main.MaskMap))
            {
                Audio.PlaySFX(SfxID.HitStone);
                Land(projectile, projectile.transform.position - projectile.Direction.normalized * 0.5f);
                return;
            }

            IActionPrimary target = HitBuffer[i].GetComponent<IActionPrimaryAttack>();
            if (target == null || projectile.SourceInfo.Machine == (Machine)target) continue;

            Info targetInfo = ((Machine)target).GetModule<Info>();
            if (targetInfo.OnHitInternal(projectile))
            {
                OnHitTarget(projectile, (Machine)target);
                Land(projectile, projectile.transform.position);
                return;
            }
        }
    }
}  