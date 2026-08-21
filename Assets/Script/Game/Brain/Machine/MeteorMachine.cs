/// <summary>A fallen meteor — a rare, very tough node that yields meteorite ore.</summary>
public class MeteorMachine : StructureMachine
{
    public static Info CreateInfo()
    {
        return new SpriteStructureInfo() {
            Health = 120,
            Loot = ID.Meteor,
            SfxHit = SfxID.HitMetal,
            SfxDestroy = SfxID.HitMetal,
            operationType = OperationType.Mining,
            SpawnsRubble = false,
        }; 
    }
}
