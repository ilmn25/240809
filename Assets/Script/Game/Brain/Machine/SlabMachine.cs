public class SlabMachine : DestructableMachine
{
    public static Info CreateInfo()
    {
        return new DestructableInfo {
            Health = 60,
            Loot = "slab",
            SfxHit = "dig_metal",
            SfxDestroy = "dig_metal",
        }; 
    }
}