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
    public bool IsGrown;

    public override void Update()
    {
        // Growth is driven by Environment.HourlyTriggered in PlanterMachine.
    }
}
