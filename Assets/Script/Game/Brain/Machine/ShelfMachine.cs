/// <summary>A wooden wall shelf that works like a chest: opening it shows a
/// 9-slot storage, and breaking it drops the shelf plus everything stored inside.</summary>
public class ShelfMachine : ChestMachine
{
    public static Info CreateInfo()
    {
        return new ContainerInfo
        {
            Health = 500,
            Loot = ID.Shelf,
            SfxHit = SfxID.HitStone,
            SfxDestroy = SfxID.HitStone,
            Storage = new Storage(9),
        };
    }
}
