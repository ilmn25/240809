using System;
using UnityEngine;

[System.Serializable]
public class BlockInfo : StructureInfo
{
    private Vector3Int Coordinate => Vector3Int.FloorToInt(position);

    public override void Initialize()
    {
        Terraform.PendingBlocks.Add(Coordinate);
        Block block;
        if (id == ID.ChalkPowder)
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
        if (id == ID.ChalkPowder)
            World.SetBlock(Vector3Int.FloorToInt(position));
        else
            World.SetBlock(Vector3Int.FloorToInt(position), Block.ConvertID(id));
        Terraform.PendingBlocks.Remove(Vector3Int.FloorToInt(Machine.transform.position)); 
    }

    public override string ToString()
    {
        string name;
        if (id != ID.ChalkPowder)
        {
            name = Helper.ToDisplayName(id);
        }
        else
        {
            Block block = Block.GetBlock(World.GetBlock(Coordinate));
            name = block != null ? Helper.ToDisplayName(block.StringID) : Helper.ToDisplayName(id);
        }

        if (operationType != OperationType.Building)
        {
            string action = operationType switch
            {
                OperationType.Mining => "Mining",
                OperationType.Building => "Building",
                OperationType.Cutting => "Cutting",
                _ => "Destroying",
            };
            return $"{action} {name} | HP {Health}";
        }

        return $"Building: {name} | {Health:0.#} Left";
    }
}
 