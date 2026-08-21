/// <summary>A hardy oak with a thick trunk — needs a strong axe (MetalAxe or better) to fell.</summary>
public class OakTreeMachine : TreeMachine
{
    public static Info CreateInfo()
    {
        return CreateInfo(ID.OakTree, threshold: 2, health: 80);
    }
}
