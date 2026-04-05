public abstract class TreeMachine : StructureMachine
{
    protected static Info CreateInfo(ID loot)
    {
        return new SpriteStructureInfo() {
            Health = 40,
            Loot = loot,
            SfxHit = SfxID.HitStone,
            SfxDestroy = SfxID.HitStone,
            operationType = OperationType.Cutting,
        }; 
    }
}