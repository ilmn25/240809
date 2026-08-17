using System;
using UnityEngine;

[Serializable]
public class PlanterInfo : SpriteStructureInfo
{
    public ID SeedID = ID.Acorn;
    public ID HarvestID = ID.Log;
    public int HarvestAmount = 1;
    public int GrowAtDay = -1;
    public int GrowAtHour = -1;
    public bool IsPlanted;
    public bool IsWatered;
    public bool IsGrown;

    public override bool OnHitInternal(Projectile projectile)
    {
        if (TryWaterWithBucket(projectile.SourceInfo))
            return true;
        return base.OnHitInternal(projectile);
    }

    private bool TryWaterWithBucket(MobInfo source)
    {
        if (source is not PlayerInfo player ||
            player.Storage?.List == null || player.Storage.List.Count == 0)
            return false;
        if (Machine is not PlanterMachine planter || !planter.CanWater())
            return false;

        int key = Mathf.Clamp(player.Storage.Key, 0, player.Storage.List.Count - 1);
        ItemSlot selected = player.Storage.List[key];
        if (selected.Stack <= 0 || selected.ID != ID.BucketOfWater)
            return false;

        player.Storage.List[key] = new ItemSlot(ID.Bucket, 1);
        player.Storage.NotifyChanged();
        planter.Water();
        return true;
    }
}
