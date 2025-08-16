using System.Collections.Generic;
using UnityEngine;
public class MapEdit
{
    private static List<(Vector3Int coordinate, int breakCost, int breakThreshold)> _blockDataList = new List<(Vector3Int, int, int)>();
    
    public static void BreakBlock(Vector3Int coordinate, int breakValue)
    {
        if (!World.IsInWorldBounds(coordinate)) return; 
        if (World.GetBlock(coordinate) == 0) return;
            
        // Check if the coordinates already exist in the list
        var existingBlockData = _blockDataList.Find(data => data.coordinate == coordinate);

        int breakCost, breakThreshold;
        string blockNameID;
        if (existingBlockData != default)
        {
            breakCost = existingBlockData.breakCost;
            breakThreshold = existingBlockData.breakThreshold;
            blockNameID = Block.ConvertID(World.GetBlock(coordinate));
        }
        else
        {
            blockNameID = Block.ConvertID(World.GetBlock(coordinate)); 
            Block targetBlockData = Block.GetBlock(blockNameID);
            breakCost = targetBlockData.BreakCost;
            breakThreshold = targetBlockData.BreakThreshold;
        }

        if (breakValue >= breakThreshold)
        { 
            breakCost -= breakValue;

            if (breakCost <= 0)
            {
                Entity.SpawnItem(blockNameID, coordinate);
                Audio.PlaySFX("dig_stone");
                World.SetBlock(coordinate, 0);
                _blockDataList.Remove(existingBlockData);
            }
            else
            {
                Audio.PlaySFX("dig_stone");
                _blockDataList.Remove(existingBlockData);
                _blockDataList.Add((coordinate, breakCost, breakThreshold));
            }
        }
    }
}
