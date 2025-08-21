public class StationMachine: CraftingMachine
{
    public static Info CreateInfo()
    {
        Storage storage = new Storage(9);
        storage.AddItem(ID.Blueprint);   
        storage.AddItem(ID.Workbench);
        storage.AddItem(ID.Sawmill);
        storage.AddItem(ID.Stonecutter);
        storage.AddItem(ID.Furnace); 
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
        storage.AddItem(ID.SteelSword);
        storage.AddItem(ID.MetalAxe);
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