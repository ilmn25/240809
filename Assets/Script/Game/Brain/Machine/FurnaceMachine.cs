public class FurnaceMachine: CraftingMachine
{
    protected override bool GlowsAlways => true;

    public static Info CreateInfo()
    {
        return CraftInfo.CreateStructureInfo(ID.Furnace, 500, SfxID.HitStone, SfxID.HitStone);
    }
}