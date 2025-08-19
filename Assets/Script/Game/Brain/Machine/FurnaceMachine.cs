public class FurnaceMachine: CraftingMachine
{
    public static Info CreateInfo()
    {
        Storage storage = new Storage(9);
        storage.AddItem(ID.Charcoal);   
        storage.AddItem(ID.Steel);    
        storage.AddItem(ID.Slag);    
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