using UnityEngine;

/// <summary>Lich enemy. Its stats and crush projectile are registered as hostile melee
/// data in <see cref="Entity.Initialize"/>; the only behaviour of its own is that it
/// stalks during the Rapture and vanishes when that weather ends.</summary>
public class LichMachine : HostileMeleeMachine
{
    public override void OnUpdate()
    {
        // The Rapture ends — vanish in a puff.
        if (Save.Inst.weather != EnvironmentType.Rapture)
        {
            if (!Info.Destroyed)
            {
                Particle.Create(transform.position, Particles.HitDust, true);
                Info.Destroy();
            }
            return;
        }

        base.OnUpdate();
    }
}
