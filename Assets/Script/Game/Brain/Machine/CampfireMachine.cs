public class CampfireMachine: CraftingMachine
{
    public static Info CreateInfo()
    {
        Storage storage = new NoRefreshStorage(9);
        storage.CreateAndAddItem(ID.Charcoal);
        storage.CreateAndAddItem(ID.CookedMeat);
        return new CraftInfo()
        {
            Health = 500,
            Loot = ID.Campfire,
            SfxHit = SfxID.HitStone,
            SfxDestroy = SfxID.HitStone,
            Storage = storage
        };
    }
}
