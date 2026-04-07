public class CampfireMachine: CraftingMachine
{
    public static Info CreateInfo()
    {
        return CraftInfo.CreateStructureInfo(ID.Campfire, 500, SfxID.HitStone, SfxID.HitStone);
    }
}
