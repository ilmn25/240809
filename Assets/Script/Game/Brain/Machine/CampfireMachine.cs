public class CampfireMachine: CraftingMachine
{
    protected override bool GlowsAlways => true;

    public static Info CreateInfo()
    {
        return CraftInfo.CreateStructureInfo(ID.Campfire, 500, SfxID.HitStone, SfxID.HitStone);
    }
}
