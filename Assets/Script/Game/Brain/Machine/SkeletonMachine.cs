using UnityEngine;

public class SkeletonMachine : StructureMachine
{
    public static Info CreateInfo()
    {
        return new SpriteStructureInfo()
        {
            Health = 30,
            Loot = ID.Skeleton,
            SfxHit = SfxID.HitStone,
            SfxDestroy = SfxID.HitStone,
            operationType = OperationType.Mining,
        };
    }
}
