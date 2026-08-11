using System;

/// <summary>Immortal enemy info. Takes damage but can never drop below 1 HP,
/// so it reacts to hits (targets the attacker, knocks back) but can't be killed.</summary>
[System.Serializable]
public class LichInfo : EnemyInfo
{
    public override bool OnHitInternal(Projectile projectile)
    {
        bool hit = base.OnHitInternal(projectile);
        Health = Math.Max(1, Health); // literally cannot die
        return hit;
    }
}