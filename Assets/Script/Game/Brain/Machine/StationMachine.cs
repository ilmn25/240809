public class StationMachine: CraftingMachine
{
    public static Info CreateInfo()
    {
        Storage storage = new Storage(9);
        storage.CreateAndAddItem(ID.Blueprint);   
        storage.CreateAndAddItem(ID.Workbench);
        storage.CreateAndAddItem(ID.Sawmill);
        storage.CreateAndAddItem(ID.Stonecutter);
        storage.CreateAndAddItem(ID.Furnace); 
        return new ContainerInfo()
        {
            Health = 500,
            Loot = ID.Tree,
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
        return new ContainerInfo()
        {
            Health = 500,
            Loot = ID.Tree,
            SfxHit = SfxID.HitStone,
            SfxDestroy = SfxID.HitStone,
            Storage = storage
        };
    }
}