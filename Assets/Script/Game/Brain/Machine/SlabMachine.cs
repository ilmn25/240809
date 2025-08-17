public class SlabMachine : DestructableMachine
{
    public static Info CreateInfo()
    {
        return new ResourceInfo() {
            Health = 60,
            Loot = "slab",
            SfxHit = "dig_metal",
            SfxDestroy = "dig_metal",
        }; 
    }
}