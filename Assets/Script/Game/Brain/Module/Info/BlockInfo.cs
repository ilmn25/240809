using System;
using UnityEngine;

[System.Serializable]
public class BlockInfo : StructureInfo
{
    private Vector3Int Coordinate => Vector3Int.FloorToInt(position);
    public override void Initialize()
    {
        Terraform.PendingBlocks.Add(Coordinate);
        PlayerTask.Pending.Add(this);
        Block block;
        if (id == ID.Blueprint)
        {
            block = Block.GetBlock(World.GetBlock(Coordinate));
            operationType = OperationType.Mining;
            Loot = block.StringID; 
        }
        else
        { 
            block = Block.GetBlock(id);
            operationType = OperationType.Building;  
        }
        
        Health = block.BreakCost;
        threshold = block.BreakThreshold;
        SfxHit = SfxID.HitMetal;
        SfxDestroy = SfxID.HitMetal;
    }

    public override void OnDestroy(MobInfo info)
    { 
        if (id == ID.Blueprint)
            World.SetBlock(Vector3Int.FloorToInt(position));
        else
            World.SetBlock(Vector3Int.FloorToInt(position), Block.ConvertID(id));
        Terraform.PendingBlocks.Remove(Vector3Int.FloorToInt(Machine.transform.position)); 
    }
}
 