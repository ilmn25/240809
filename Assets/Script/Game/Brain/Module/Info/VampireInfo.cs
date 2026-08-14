using System;

/// <summary>Info for the vampire. It can only be damaged by fire: normal projectiles
/// are ignored entirely, while the Burn status effect (from igniting with a torch)
/// deals damage over time. On being hit it can teleport away to dodge.</summary>
[System.Serializable]
public class VampireInfo : EnemyInfo
{
    public override void Initialize()
    {
        base.Initialize();
        Flammable = true; // ignitable by torch so fire can hurt it
    }

    /// <summary>Ignore all direct projectile damage — the vampire is immune to
    /// everything except fire (the Burn status effect, which bypasses this).
    /// The hit still plays its animation and queues a teleport dodge, but deals
    /// no damage.</summary>
    public override bool OnHitInternal(Projectile projectile)
    {
        // Play the normal hit reaction (animation, knockback, target acquire) but
        // remember the health before so we can undo the non-fire damage.
        int healthBefore = Health;
        bool hit = base.OnHitInternal(projectile);
        if (hit)
        {
            // Undo the damage — the vampire is immune to non-fire hits.
            Health = healthBefore;

            // Queue a teleport dodge that fires after the hit animation finishes.
            if (Machine is VampireMachine vampire)
                vampire.OnVampireHit();
        }
        return hit;
    }
}