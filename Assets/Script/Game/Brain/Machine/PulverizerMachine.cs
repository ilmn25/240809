public class PulverizerMachine : CraftingMachine
{
    public static Info CreateInfo()
    {
        return new PulverizerInfo()
        {
            Health = 500,
            Loot = ID.Pulverizer,
            Sfx = SfxID.HitStone,
            SfxHit = SfxID.HitStone,
            SfxDestroy = SfxID.HitStone,
        };
    }
}
