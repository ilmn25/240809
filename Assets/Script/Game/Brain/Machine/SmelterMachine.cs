public class SmelterMachine : CraftingMachine
{
    protected override bool GlowsWhenCrafting => true;

    public static Info CreateInfo()
    {
        return CraftInfo.CreateStructureInfo(ID.Smelter, 500, SfxID.HitStone, SfxID.HitStone);
    }
}
