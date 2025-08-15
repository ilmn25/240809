public class TreeMachine : DestructableMachine
{
    public static Info CreateInfo()
    {
        return new StructureInfo {
            Health = 15,
            Loot = "tree",
            SfxHit = "dig_stone",
            SfxDestroy = "dig_stone",
        }; 
    }
}