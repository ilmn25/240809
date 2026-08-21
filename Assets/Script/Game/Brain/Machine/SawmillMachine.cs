public class SawmillMachine: CraftingMachine
{
    public static Info CreateInfo()
    {
        return CraftInfo.CreateStructureInfo(ID.Sawmill, 500, SfxID.HitStone, SfxID.HitStone);
    }
}