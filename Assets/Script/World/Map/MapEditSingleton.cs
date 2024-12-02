using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MapEditSingleton : MonoBehaviour
{
    public static MapEditSingleton Instance { get; private set; }  
    private List<(Vector3Int coordinate, int breakCost, int breakThreshold)> blockDataList;
    
    void Start()
    {
        Instance = this;
        blockDataList = new List<(Vector3Int, int, int)>();
    }

    public void BreakBlock(Vector3Int coordinate, int breakValue)
    {
        
        // Check if the coordinates already exist in the list
        var existingBlockData = blockDataList.Find(data => data.coordinate == coordinate);

        int breakCost;
        int breakThreshold;
        string blockNameID;
        if (existingBlockData != default)
        {
            // Use existing breakCost and breakThreshold
            breakCost = existingBlockData.breakCost;
            breakThreshold = existingBlockData.breakThreshold;
            blockNameID = BlockSingleton.ConvertID(MapLoadSingleton.Instance.GetBlockInChunk(coordinate));
        }
        else
        {
            // Check if the block is occupied
            blockNameID = BlockSingleton.ConvertID(MapLoadSingleton.Instance.GetBlockInChunk(coordinate));
            if (blockNameID == null)
            {
                return; // Block is not occupied or an error occurred
            }

            // Get the block value
            BlockData targetBlockData = BlockSingleton.GetBlock(blockNameID);
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
                Entity.SpawnItem(blockNameID, Lib.AddToVector(coordinate, 0.5f, 0, 0.5f));
                WorldSingleton.Instance.UpdateMap(coordinate, 0);
                blockDataList.Remove(existingBlockData);
            }
            else
            {
                // Update the block data in the list
                blockDataList.Remove(existingBlockData);
                blockDataList.Add((coordinate, breakCost, breakThreshold));
            }
        }
    }
}
