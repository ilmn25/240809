public class SawmillMachine: CraftingMachine
{
    public static Info CreateInfo()
    {
        Storage storage = new NoRefreshStorage(9);
        storage.CreateAndAddItem(ID.Plank);
        storage.CreateAndAddItem(ID.Stake);
        storage.CreateAndAddItem(ID.Chest);   
        return new CraftInfo()
        {
            Health = 500,
            Loot = ID.Sawmill,
            SfxHit = SfxID.HitStone,
            SfxDestroy = SfxID.HitStone,
            Storage = storage
        };
    }
}