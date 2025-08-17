public class BlockMachine : EntityMachine, IHitBoxResource
{
    public new BlockInfo Info => GetModule<BlockInfo>();
    public static Info CreateInfo()
    {
        return new BlockInfo();
    }
}