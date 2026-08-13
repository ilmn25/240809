using System;

/// <summary>Info for the visitor. On top of normal passive behavior, being hit
/// enrages the visitor — it locks onto the attacker and becomes lethal.</summary>
[System.Serializable]
public class VisitorInfo : PassiveInfo
{
    protected override void OnHit(Projectile projectile)
    {
        base.OnHit(projectile);
        if (Machine is VisitorMachine visitor)
            visitor.Enrage(projectile.SourceInfo);
    }
}
