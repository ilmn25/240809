public class BlockMachine : EntityMachine, IActionPrimaryResource
{
    public new BlockInfo Info => GetModule<BlockInfo>();
    public static Info CreateInfo()
    {
        return new BlockInfo();
    }
}