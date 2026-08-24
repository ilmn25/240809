/// <summary>An old pot that, when smashed, may pop out a viper or spill loot.</summary>
public class OldPotMachine : HarvestableMachine
{
    public static new Info CreateInfo()
    {
        return new OldPotInfo()
        {
            Health = 8,
            SfxHit = SfxID.HitStone,
            SfxDestroy = SfxID.HitStone,
            operationType = OperationType.Mining,
            threshold = 1,
            SpawnsRubble = false,
        };
    }
}
