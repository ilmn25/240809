public class SandPileMachine : StructureMachine
{
    public static Info CreateInfo()
    {
        return new SpriteStructureInfo()
        {
            Health = 35,
            Loot = ID.SandPile,
            SfxHit = SfxID.HitSand,
            SfxDestroy = SfxID.HitSand,
            operationType = OperationType.Mining,
            SpawnsRubble = false,
        };
    }
}
