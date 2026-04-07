public class FurnaceMachine: CraftingMachine
{
    public static Info CreateInfo()
    {
        return CraftInfo.CreateStructureInfo(ID.Furnace, 500, SfxID.HitStone, SfxID.HitStone);
    }
}