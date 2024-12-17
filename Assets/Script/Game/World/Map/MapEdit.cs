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
            // Use existing breakCost and breakThreshold
            breakCost = existingBlockData.breakCost;
            breakThreshold = existingBlockData.breakThreshold;
            blockNameID = Block.ConvertID(World.GetBlock(coordinate));
        }
        else
        {
            // Check if the block is occupied
            blockNameID = Block.ConvertID(World.GetBlock(coordinate));
            if (blockNameID == null)
            {
                return; // Block is not occupied or an error occurred
            }

            // Get the block value
            BlockData targetBlockData = Block.GetBlock(blockNameID);
            breakCost = targetBlockData.BreakCost;
            breakThreshold = targetBlockData.BreakThreshold;
            
            // Add the block data to the list
            // blockDataList.Add((chunkCoordinate, blockCoordinate, breakCost, breakThreshold));
        }

        // Check if the break value is above the threshold
        if (breakValue >= breakThreshold)
        {
            // Deduct the break cost
            breakCost -= breakValue;

            // If the cost reaches 0 or below, break the block and remove from the list
            if (breakCost <= 0)
            {
                Entity.SpawnItem(blockNameID, coordinate);
                Audio.PlaySFX(Game.DigSound);
                World.SetBlock(coordinate, 0);
                _blockDataList.Remove(existingBlockData);
            }
            else
            {
                // Update the block data in the list
                _blockDataList.Remove(existingBlockData);
                _blockDataList.Add((coordinate, breakCost, breakThreshold));
            }
        }
    }
}
