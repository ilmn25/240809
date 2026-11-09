/// <summary>A charred tree left behind after a tree burns out. Can be chopped for charcoal.</summary>
public class BurnedTreeMachine : StructureMachine
{
    public static Info CreateInfo()
    {
        return new SpriteStructureInfo()
        {
            Health = 30,
            Loot = ID.BurnedTree,
            SfxHit = SfxID.HitStone,
            SfxDestroy = SfxID.HitStone,
            operationType = OperationType.Cutting,
            SpawnsRubble = false,
        };
    }
}
