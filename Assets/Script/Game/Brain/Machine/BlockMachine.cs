using UnityEngine;

public class BlockMachine : EntityMachine, IActionPrimaryResource, IActionSecondaryInteract
{
    public new BlockInfo Info => GetModule<BlockInfo>();
    public static Info CreateInfo()
    {
        return new BlockInfo();
    }

    public override void OnSetup()
    {
        if (Info.id == ID.Blueprint)
        {
            transform.localScale = Vector3.one * 1.04f; 
            BlockPreview.Set(gameObject, ID.OverlayBlock); 
        }
        else
        {
            transform.localScale = Vector3.one; 
            BlockPreview.Set(gameObject, Info.id);
        }
    }
    
    public void OnActionSecondary(Info info)
    {
        if (Info.id != ID.Blueprint) Entity.SpawnItem(Info.id, transform.position);
        Terraform.PendingBlocks.Remove(Vector3Int.FloorToInt(transform.position));
        Audio.PlaySFX(SfxID.Item);
        Info.Destroy();
    }
} 