using System;
using UnityEngine;

[System.Serializable]
public class BlockInfo : StructureInfo
{
    private Vector3Int Coordinate => Vector3Int.FloorToInt(position);

    public override void Initialize()
    {
        Terraform.PendingBlocks.Add(Coordinate);
        bool miningBox = id == ID.MiningBox;
        Block block = miningBox ? Block.GetBlock(World.GetBlock(Coordinate)) : Block.GetBlock(id);
        operationType = miningBox ? OperationType.Mining : OperationType.Building;
        if (miningBox) Loot = block.StringID;
        Health = block.BreakCost;
        threshold = block.BreakThreshold;
        SfxHit = SfxID.HitMetal;
        SfxDestroy = SfxID.HitMetal;
    }

    public override void OnDestroy(MobInfo info)
    {
        World.SetBlock(Coordinate, id == ID.MiningBox ? 0 : Block.ConvertID(id));
        Terraform.PendingBlocks.Remove(Coordinate);
    }

    public override string ToString()
    {
        Block underlying = id == ID.MiningBox ? Block.GetBlock(World.GetBlock(Coordinate)) : null;
        string name = underlying != null ? Helper.ToDisplayName(underlying.StringID) : Helper.ToDisplayName(id);
        return operationType != OperationType.Building
            ? $"Mining {name} | HP {Health}"
            : $"Building: {name} | {Health:0.#} Left";
    }
}
 