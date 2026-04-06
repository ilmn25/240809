public class StonecutterMachine: CraftingMachine
{
    public static Info CreateInfo()
    {
        Storage storage = new NoRefreshStorage(9);
        storage.CreateAndAddItem(ID.Brick);
        storage.CreateAndAddItem(ID.BrickBlock); 
        return new CraftInfo()
        {
            Health = 500,
            Loot = ID.Stonecutter,
            SfxHit = SfxID.HitStone,
            SfxDestroy = SfxID.HitStone,
            Storage = storage
        };
    }
}