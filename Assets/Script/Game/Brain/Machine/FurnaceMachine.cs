public class FurnaceMachine: ConverterMachine
{
    public static Info CreateInfo()
    {
        Storage storage = new NoRefreshStorage(3);
        storage.CreateAndAddItem(ID.Slag);    
        storage.CreateAndAddItem(ID.Steel);    
        return new ConverterInfo()
        {
            Health = 500,
            Loot = ID.PineTree,
            SfxHit = SfxID.HitStone,
            SfxDestroy = SfxID.HitStone,
            Storage = storage
        };
    }
}