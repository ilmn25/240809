using UnityEngine;

/// <summary>Info for the spider web. On top of normal harvestable behavior,
/// destroying it alerts nearby spiders (like stepping on it does).</summary>
[System.Serializable]
public class SpiderWebInfo : HarvestableInfo
{
    public override void OnDestroy(MobInfo info)
    {
        if (Machine is SpiderWebMachine web)
            web.AlertSpiders();
    }
}
