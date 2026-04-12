public class SandDebrisMachine : StructureMachine
{
    public static Info CreateInfo()
    {
        return new SpriteStructureInfo()
        {
            Health = 25,
            Loot = ID.SandDebris,
            SfxHit = SfxID.HitSand,
            SfxDestroy = SfxID.HitSand,
            operationType = OperationType.Mining,
        };
    }
}
