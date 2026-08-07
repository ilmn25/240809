using UnityEngine;

/// <summary>
/// A melee swing that, instead of dealing damage, ignites flammable objects
/// it touches (Don't Starve style torch). Any structure with a FlammableModule
/// catches fire when struck.
/// </summary>
public class TorchProjectileInfo : SwingProjectileInfo
{
    public TorchProjectileInfo() { Class = ProjectileClass.Melee; }

    public override void AI(Projectile projectile)
    {
        Vector3 direction = (projectile.Destination - projectile.transform.position).normalized;
        Vector3 center = projectile.transform.position + direction * (Radius * 0.5f);
        int hitCount = Physics.OverlapSphereNonAlloc(center, Radius / 2, HitBuffer, Main.MaskEntity);

        for (int i = 0; i < hitCount; i++)
        {
            // Ignite any flammable object we touch. FlammableModule is an EntityModule
            // on the Machine (a MonoBehaviour), so resolve the machine first.
            Machine machine = HitBuffer[i].GetComponent<Machine>();
            if (machine != null)
            {
                FlammableModule flammable = machine.GetModule<FlammableModule>();
                if (flammable != null && flammable.Ignite())
                    Audio.PlaySFX(SfxID.Sword);
            }

            // Also register the hit so the swing still feels responsive.
            IActionPrimary target = HitBuffer[i].GetComponent<IActionPrimary>();
            if (target == null || projectile.SourceInfo.Machine == (Machine)target) continue;
            Info info = ((Machine)target).GetModule<Info>();
            info.OnHitInternal(projectile);
        }
        projectile.Delete();
    }
}
