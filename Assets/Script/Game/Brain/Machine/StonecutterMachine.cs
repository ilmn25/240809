public class StonecutterMachine: CraftingMachine
{
    public static Info CreateInfo()
    {
        return CraftInfo.CreateStructureInfo(ID.Stonecutter, 500, SfxID.HitStone, SfxID.HitStone);
    }
}