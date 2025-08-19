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
        Delete();
    }
}
public class BreakBlockMachine : EntityMachine, IActionPrimaryResource, IActionSecondaryInteract
{
    public new BreakBlockInfo Info => GetModule<BreakBlockInfo>();
    public static Info CreateInfo()
    {
        return new BreakBlockInfo();
    }

    public void OnActionSecondary(Info info)
    {
        PlayerTerraformModule.Position.Remove(Vector3Int.FloorToInt(this.transform.position));
        Delete();
    }
}