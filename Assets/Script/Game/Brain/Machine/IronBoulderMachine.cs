/// <summary>A heavy iron boulder — slow to mine, but rich in metal ore.</summary>
public class IronBoulderMachine : StructureMachine
{
    public static Info CreateInfo()
    {
        return new SpriteStructureInfo()
        {
            Health = 80,
            Loot = ID.IronBoulder,
            SfxHit = SfxID.HitMetal,
            SfxDestroy = SfxID.HitMetal,
            operationType = OperationType.Mining,
            SpawnsRubble = false,
        };
    }
}
