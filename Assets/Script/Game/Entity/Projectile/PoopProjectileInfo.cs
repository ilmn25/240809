using UnityEngine;

/// <summary>A falling bird-shit projectile dropped by a pigeon. It falls straight
/// down and damages whatever it lands on, then disappears.</summary>
public class PoopProjectileInfo : ProjectileInfo
{
    public int LifeSpan = 120; // frames before it despawns if it never lands

    public override void AI(Projectile projectile)
    {
        if (projectile.LifeSpan > LifeSpan)
        {
            projectile.Delete();
            return;
        }

        // Fall straight down.
        projectile.transform.position += Vector3.down * (Speed * Time.deltaTime);

        int hitCount = Physics.OverlapSphereNonAlloc(projectile.transform.position, Radius, HitBuffer);
        for (int i = 0; i < hitCount; i++)
        {
            // Land on the ground — leave a pickupable pile of droppings behind.
            if (Helper.IsInLayerMask(HitBuffer[i].gameObject, Main.MaskMap))
            {
                Audio.PlaySFX(SfxID.HitStone);
                Entity.SpawnItem(ID.BirdShit, projectile.transform.position);
                Particle.Create(projectile.transform.position, Particles.HitDust, false);
                projectile.Delete();
                return;
            }

            // Hit an entity.
            IActionPrimary target = HitBuffer[i].GetComponent<IActionPrimary>();
            if (target == null || projectile.SourceInfo.Machine == (Machine)target) continue;
            if (((Machine)target).GetModule<Info>().OnHitInternal(projectile))
            {
                projectile.Delete();
                return;
            }
        }
    }
}
