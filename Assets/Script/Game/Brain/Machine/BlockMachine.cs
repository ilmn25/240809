using UnityEngine;

public class BlockMachine : EntityMachine, IActionPrimaryResource, IActionSecondaryInteract
{
    public new BlockInfo Info => GetModule<BlockInfo>();
    public static Info CreateInfo()
    {
        return new BlockInfo();
    }

    public void OnActionSecondary(Info info)
    {
        Entity.SpawnItem(Info.blockID, transform.position);
        PlayerTerraformModule.Position.Remove(Vector3Int.FloorToInt(this.transform.position));
        Audio.PlaySFX(SfxID.Item);
        Info.Destroy();
    }
}
public class BreakBlockMachine : EntityMachine, IActionPrimaryResource, IActionSecondaryInteract
{
    public new BreakBlockInfo Info => GetModule<BreakBlockInfo>();
    public static Info CreateInfo()
    {
        return new BreakBlockInfo();
    }

    public override void OnSetup()
    {
        transform.localScale = Vector3.one * 1.04f; 
        BlockPreview.Set(gameObject, ID.OverlayBlock);
    }

    public void OnActionSecondary(Info info)
    {
        PlayerTerraformModule.Position.Remove(Vector3Int.FloorToInt(this.transform.position));
        Audio.PlaySFX(SfxID.Item);
        Info.Destroy();
    }
}