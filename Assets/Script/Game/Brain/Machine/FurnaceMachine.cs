public class FurnaceMachine: CraftingMachine
{
    public static Info CreateInfo()
    {
        Storage storage = new NoRefreshStorage(9);
        storage.CreateAndAddItem(ID.Slag);    
        storage.CreateAndAddItem(ID.Steel);    
        return new CraftInfo()
        {
            Health = 500,
            Loot = ID.Furnace,
            SfxHit = SfxID.HitStone,
            SfxDestroy = SfxID.HitStone,
            Storage = storage
        };
    }
}