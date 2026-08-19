/// <summary>Charred rubble left behind when a wooden structure burns out. Can be mined for gravel.</summary>
public class CharredRubbleMachine : StructureMachine
{
    public static Info CreateInfo()
    {
        return new SpriteStructureInfo()
        {
            Health = 40,
            Loot = ID.Gravel,
            SfxHit = SfxID.HitStone,
            SfxDestroy = SfxID.HitStone,
            operationType = OperationType.Mining,
        };
    }
}
