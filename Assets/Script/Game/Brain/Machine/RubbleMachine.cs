/// <summary>Rubble left behind when a structure is broken. Can be mined for gravel.</summary>
public class RubbleMachine : StructureMachine
{
    public static Info CreateInfo()
    {
        return new SpriteStructureInfo()
        {
            Health = 40,
            Loot = ID.Rubble,
            SfxHit = SfxID.HitStone,
            SfxDestroy = SfxID.HitStone,
            operationType = OperationType.Mining,
        };
    }
}
