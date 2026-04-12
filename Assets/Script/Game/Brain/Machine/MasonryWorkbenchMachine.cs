public class MasonryWorkbenchMachine : CraftingMachine
{
    public static Info CreateInfo()
    {
        return CraftInfo.CreateStructureInfo(ID.MasonryWorkbench, 500, SfxID.HitStone, SfxID.HitStone);
    }
}
