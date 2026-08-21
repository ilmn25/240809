public abstract class TreeMachine : StructureMachine
{
    protected static Info CreateInfo(ID loot, int threshold = 1, float health = 40)
    {
        return new SpriteStructureInfo() {
            Health = health,
            threshold = threshold,
            Loot = loot,
            SfxHit = SfxID.HitStone,
            SfxDestroy = SfxID.HitStone,
            operationType = OperationType.Cutting,
            SpawnsRubble = false,
        }; 
    }
}