public class RefineryMachine : ConverterMachine
{
    public static Info CreateInfo()
    {
        return new ExtractionInfo()
        {
            Health = 500,
            Loot = ID.Refinery,
            SfxHit = SfxID.HitStone,
            SfxDestroy = SfxID.HitStone,
            Storage = new Storage(9),
        };
    }
}
