public class SandSlabMachine : StructureMachine
{
    public static Info CreateInfo()
    {
        return new SpriteStructureInfo()
        {
            Health = 35,
            Loot = ID.SandSlab,
            SfxHit = SfxID.HitSand,
            SfxDestroy = SfxID.HitSand,
            operationType = OperationType.Mining,
        };
    }
}
