using UnityEngine;

/// <summary>
/// A melee swing that, instead of dealing damage, ignites flammable entities
/// it touches (like a torch). Any flammable entity (structure, mob, player)
/// catches fire when struck — except a friendly NPC, which a friendly swing
/// only shoves aside.
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
            Info target = machine.GetModule<Info>();
            if (target is not DynamicInfo dynamicInfo || !dynamicInfo.IgnoresFriendlyHit(projectile.SourceInfo))
            {
                FlammableModule flammable = machine.GetModule<FlammableModule>();
                if (flammable != null && flammable.Ignite())
                    Audio.PlaySFX(SfxID.Sword);
            }
            target.OnHitInternal(projectile);
        });
        projectile.Delete();
    }
}
