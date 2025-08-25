public class TreeMachine : StructureMachine
{
    public static Info CreateInfo()
    {
        return new SpriteStructureInfo() {
            Health = 100,
            Loot = ID.Tree,
            SfxHit = SfxID.HitStone,
            SfxDestroy = SfxID.HitStone,
            operationType = OperationType.Cutting,
        }; 
    }
}