public class StonecutterMachine: ConverterMachine
{
    public static Info CreateInfo()
    {
        Storage storage = new Storage(3);
        storage.AddItem(ID.Brick);
        storage.AddItem(ID.BrickBlock); 
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
public class SawmillMachine: ConverterMachine
{
    public static Info CreateInfo()
    {
        Storage storage = new Storage(3);
        storage.AddItem(ID.Plank);
        storage.AddItem(ID.Stake);
        storage.AddItem(ID.Chest);   
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
public class CampfireMachine: ConverterMachine
{
    public static Info CreateInfo()
    {
        Storage storage = new Storage(3);
        storage.AddItem(ID.Charcoal);
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