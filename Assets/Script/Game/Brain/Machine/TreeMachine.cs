public class TreeMachine : DestructableMachine
{
    public static Info CreateInfo()
    {
        return new DestructableInfo {
            Health = 100,
            Loot = "tree",
            SfxHit = "dig_stone",
            SfxDestroy = "dig_stone",
        }; 
    }
}