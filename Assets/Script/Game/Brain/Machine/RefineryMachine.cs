public class RefineryMachine : ConverterMachine
{
    public static Info CreateInfo()
    {
        return new RefineryInfo()
        {
            Health = 500,
            Loot = ID.Refinery,
            SfxHit = SfxID.HitStone,
            SfxDestroy = SfxID.HitStone,
            Storage = new Storage(9),
        };
    }
}
