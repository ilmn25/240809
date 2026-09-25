public class RockPileMachine : StructureMachine
{
    public static Info CreateInfo()
    {
        return new SpriteStructureInfo()
        {
            Health = 40,
            Loot = ID.RockPile,
            SfxHit = SfxID.HitStone,
            SfxDestroy = SfxID.HitStone,
            operationType = OperationType.Mining,
            SpawnsRubble = false,
        };
    }
}
