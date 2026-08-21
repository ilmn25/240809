/// <summary>A rich iron deposit — a tougher boulder that yields metal ore.</summary>
public class IronDepositMachine : StructureMachine
{
    public static Info CreateInfo()
    {
        return new SpriteStructureInfo() {
            Health = 80,
            Loot = ID.IronDeposit,
            SfxHit = SfxID.HitMetal,
            SfxDestroy = SfxID.HitMetal,
            operationType = OperationType.Mining,
            SpawnsRubble = false,
        }; 
    }
}
