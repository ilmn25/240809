public class TreeMachine : DestructableMachine
{
    public static Info CreateInfo()
    {
        return new ResourceInfo() {
            Health = 100,
            Loot = ID.Tree,
            SfxHit = "dig_stone",
            SfxDestroy = "dig_stone",
        }; 
    }
}