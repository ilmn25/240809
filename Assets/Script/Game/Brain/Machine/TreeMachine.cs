public class TreeMachine : DestructableMachine
{
    public static Info CreateInfo()
    {
        return new ResourceInfo() {
            Health = 100,
            Loot = ID.Tree,
            SfxHit = SfxID.HitStone,
            SfxDestroy = SfxID.HitStone,
        }; 
    }
}