public class SlabMachine : StructureMachine
{
    public static Info CreateInfo()
    {
        return new SpriteStructureInfo() {
            Health = 40,
            Loot = ID.Slab,
            SfxHit = SfxID.HitMetal,
            SfxDestroy = SfxID.HitMetal,
            operationType = OperationType.Mining,
        }; 
    }
}