public class StoneBoulderMachine : StructureMachine
{
    public static Info CreateInfo()
    {
        return new SpriteStructureInfo() {
            Health = 40,
            Loot = ID.StoneBoulder,
            SfxHit = SfxID.HitMetal,
            SfxDestroy = SfxID.HitMetal,
            operationType = OperationType.Mining,
            SpawnsRubble = false,
        }; 
    }
}
