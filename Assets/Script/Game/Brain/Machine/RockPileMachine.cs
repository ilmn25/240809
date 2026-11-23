public class RockPileMachine : HarvestableMachine
{
    public static new Info CreateInfo()
    {
        return new HarvestableInfo()
        {
            Health = 40,
            SfxHit = SfxID.HitStone,
            SfxDestroy = SfxID.HitStone,
        };
    }
}
