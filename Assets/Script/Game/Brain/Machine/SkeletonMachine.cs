using UnityEngine;

/// <summary>A skeleton remains — a harvestable that any tool can break apart for
/// bone loot (including the femur weapon).</summary>
public class SkeletonMachine : HarvestableMachine
{
    public static Info CreateInfo()
    {
        return new HarvestableInfo()
        {
            Health = 30,
            SfxHit = SfxID.HitStone,
            SfxDestroy = SfxID.HitStone,
        };
    }
}
