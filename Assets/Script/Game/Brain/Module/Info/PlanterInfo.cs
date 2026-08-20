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
}
