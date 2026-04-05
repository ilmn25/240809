public class CampfireMachine: ConverterMachine
{
    public static Info CreateInfo()
    {
        Storage storage = new NoRefreshStorage(3);
        storage.CreateAndAddItem(ID.Charcoal);
        storage.CreateAndAddItem(ID.CookedMeat);
        return new ConverterInfo()
        {
            Health = 500,
            Loot = ID.Campfire,
            SfxHit = SfxID.HitStone,
            SfxDestroy = SfxID.HitStone,
            Storage = storage
        };
    }
}
