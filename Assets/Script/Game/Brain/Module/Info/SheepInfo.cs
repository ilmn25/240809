using UnityEngine;

/// <summary>Info for sheep. Tracks whether the sheep was actually attacked
/// (retaliating) as opposed to merely fleeing from a nearby player.</summary>
[System.Serializable]
public class SheepInfo : PassiveInfo
{
    /// <summary>True when this sheep (or its herd) was attacked and should fight back.</summary>
    public bool Retaliating;

    protected override void OnHit(Projectile projectile)
    {
        base.OnHit(projectile);
        Retaliating = true;
    }
}
