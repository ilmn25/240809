public class LightningRodMachine : StructureMachine
{
    public static Info CreateInfo()
    {
        return new SpriteStructureInfo()
        {
            Health = 100,
            Loot = ID.LightningRod,
            SfxHit = SfxID.HitMetal,
            SfxDestroy = SfxID.HitMetal,
            operationType = OperationType.Mining,
            threshold = 1,
        };
    }
}
