using UnityEngine;

/// <summary>Info for the spider web. On top of normal harvestable behavior,
/// destroying it alerts nearby spiders (like stepping on it does).</summary>
[System.Serializable]
public class SpiderWebInfo : HarvestableInfo
{
    // Cutting the web down alerts spiders with the specific player who broke it,
    // so they hunt the breaker rather than just whoever is nearest.
    public override bool OnHitInternal(Projectile projectile)
    {
        if (Machine is SpiderWebMachine web)
            web.AlertSpiders(projectile.SourceInfo);
        return base.OnHitInternal(projectile);
    }
}
