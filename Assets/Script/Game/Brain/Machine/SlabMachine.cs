public class SlabMachine : DestructableMachine
{
    public static Info CreateInfo()
    {
        return new ResourceInfo() {
            Health = 60,
            Loot = ID.Slab,
            SfxHit = SfxID.HitMetal,
            SfxDestroy = SfxID.HitMetal,
        }; 
    }
}