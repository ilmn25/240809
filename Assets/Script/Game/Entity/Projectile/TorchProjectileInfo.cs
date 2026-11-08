using UnityEngine;

/// <summary>
/// A melee swing that, instead of dealing damage, ignites flammable entities
/// it touches (like a torch). Any flammable entity (structure, mob, player)
/// catches fire when struck.
/// </summary>
public class TorchProjectileInfo : SwingProjectileInfo
{
    public TorchProjectileInfo() { Class = ProjectileClass.Melee; }

    public override void AI(Projectile projectile)
    {
        Vector3 direction = (projectile.Destination - projectile.transform.position).normalized;
        Vector3 center = projectile.transform.position + direction * (Radius * 0.5f);
        ForEachMachine(projectile, center, Radius / 2, (machine, _) =>
        {
            FlammableModule flammable = machine.GetModule<FlammableModule>();
            if (flammable != null && flammable.Ignite())
                Audio.PlaySFX(SfxID.Sword);

            machine.GetModule<Info>().OnHitInternal(projectile);
        });
        projectile.Delete();
    }
}
