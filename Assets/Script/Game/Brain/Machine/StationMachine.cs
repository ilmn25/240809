public class StationMachine: CraftingMachine
{
    public static Info CreateInfo()
    {
        return CraftInfo.CreateStructureInfo(ID.Station, 500, SfxID.HitStone, SfxID.HitStone);
    }
}
public class AnvilMachine: CraftingMachine
{
    public static Info CreateInfo()
    {
        return CraftInfo.CreateStructureInfo(ID.Anvil, 500, SfxID.HitStone, SfxID.HitStone);
    }
}

public class FieldStationMachine: CraftingMachine
{
    public static Info CreateInfo()
    {
        return CraftInfo.CreateStructureInfo(ID.FieldStation, 500, SfxID.HitStone, SfxID.HitStone);
    }
}