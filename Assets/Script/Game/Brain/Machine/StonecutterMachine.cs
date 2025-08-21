public class StonecutterMachine: ConverterMachine
{
    public static Info CreateInfo()
    {
        Storage storage = new Storage(3);
        storage.CreateAndAddItem(ID.Brick);
        storage.CreateAndAddItem(ID.BrickBlock); 
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
        storage.CreateAndAddItem(ID.Plank);
        storage.CreateAndAddItem(ID.Stake);
        storage.CreateAndAddItem(ID.Chest);   
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
        storage.CreateAndAddItem(ID.Charcoal);
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