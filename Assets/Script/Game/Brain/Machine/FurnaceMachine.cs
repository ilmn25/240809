public class FurnaceMachine: ConverterMachine
{
    public static Info CreateInfo()
    {
        Storage storage = new Storage(3);
        storage.CreateAndAddItem(ID.Charcoal);   
        storage.CreateAndAddItem(ID.Slag);    
        storage.CreateAndAddItem(ID.Steel);    
        return new ConverterInfo()
        {
            Health = 500,
            Loot = ID.Tree,
            SfxHit = SfxID.HitStone,
            SfxDestroy = SfxID.HitStone,
            Storage = storage
        };
    }
}