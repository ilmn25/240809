using UnityEngine;

public class BlockMachine : EntityMachine, IActionPrimaryResource, IActionSecondaryInteract
{
    public new BlockInfo Info => GetModule<BlockInfo>();
    public static Info CreateInfo() => new BlockInfo();

    public override void OnSetup()
    {
        bool miningBox = Info.id == ID.MiningBox;
        transform.localScale = Vector3.one * (miningBox ? 1.04f : 1f);
        BlockPreview.Set(gameObject, miningBox ? ID.OverlayBlock : Info.id);
    }
    
    public void OnActionSecondary(Info info)
    {
        bool isMiningBox = Info.id == ID.MiningBox;
        if (!isMiningBox) Entity.SpawnItem(Info.id, transform.position);
        Vector3Int coord = Vector3Int.FloorToInt(transform.position);
        Terraform.PendingBlocks.Remove(coord);
        Audio.PlaySFX(SfxID.Item);
        Info.Destroy();
    }
} 