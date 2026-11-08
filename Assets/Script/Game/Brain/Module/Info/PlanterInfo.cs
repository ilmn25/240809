using System;

[Serializable]
public class PlanterInfo : SpriteStructureInfo
{
    public ID HarvestID = ID.Log;
    public int HarvestMin = 1;
    public int HarvestMax = 1;
    public int GrowAtDay = -1;
    public int GrowAtHour = -1;
    public bool IsPlanted;
    public bool IsWatered;
    public bool IsGrown;

    // Don't lift a planter out from under a crop that's still in it.
    protected override bool CanDismantleNow => !IsPlanted;
    protected override string DismantleBlockedMessage => "I should clear it out first.";
}
