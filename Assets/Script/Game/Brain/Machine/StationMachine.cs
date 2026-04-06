public class StationMachine: CraftingMachine
{
    public static Info CreateInfo()
    {
        Storage storage = new Storage(9);
        storage.CreateAndAddItem(ID.Blueprint);   
        storage.CreateAndAddItem(ID.Furnace);
        storage.CreateAndAddItem(ID.Workbench);
        storage.CreateAndAddItem(ID.Anvil);
        storage.CreateAndAddItem(ID.Sawmill);
        storage.CreateAndAddItem(ID.Stonecutter);
        return new CraftInfo()
        {
            Health = 500,
            Loot = ID.Station,
            SfxHit = SfxID.HitStone,
            SfxDestroy = SfxID.HitStone,
            Storage = storage
        };
    }
}
public class AnvilMachine: CraftingMachine
{
    public static Info CreateInfo()
    {
        Storage storage = new Storage(9);
        storage.CreateAndAddItem(ID.SteelSword);
        storage.CreateAndAddItem(ID.MetalAxe);
        return new CraftInfo()
        {
            Health = 500,
            Loot = ID.Anvil,
            SfxHit = SfxID.HitStone,
            SfxDestroy = SfxID.HitStone,
            Storage = storage
        };
    }
}