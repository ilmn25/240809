public class SlabMachine : DestructableMachine
{
    public static Info CreateInfo()
    {
        return new StructureInfo {
            Health = 10,
            Loot = "slab",
            SfxHit = "dig_metal",
            SfxDestroy = "dig_metal",
        }; 
    }
}