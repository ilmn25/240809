public class StationMachine: CraftingMachine
{
    public static Info CreateInfo()
    {
        Storage storage = new Storage(9);
        storage.AddItem(ID.Blueprint);   
        storage.AddItem(ID.Workbench);
        storage.AddItem(ID.Stonecutter);
        storage.AddItem(ID.Furnace);
        storage.AddItem(ID.Chest);   
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
public class StonecutterMachine: CraftingMachine
{
    public static Info CreateInfo()
    {
        Storage storage = new Storage(9);
        storage.AddItem(ID.BrickBlock);    
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