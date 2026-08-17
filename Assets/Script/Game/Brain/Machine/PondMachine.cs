using UnityEngine;

/// <summary>A shallow pond of liquid. Swinging an empty bucket at it fills the bucket.</summary>
public class PondMachine : StructureMachine
{
    public static Info CreateInfo()
    {
        return new PondInfo()
        {
            Health = 200,
            Loot = ID.Pond,
            SfxHit = SfxID.HitStone,
            SfxDestroy = SfxID.HitStone,
            operationType = OperationType.Mining,
            threshold = 1,
            Liquid = LiquidType.Water,
        };
    }
}
