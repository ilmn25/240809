public class SmelterMachine : CraftingMachine
{
    public static Info CreateInfo()
    {
        return CraftInfo.CreateStructureInfo(ID.Smelter, 500, SfxID.HitStone, SfxID.HitStone);
    }
}
