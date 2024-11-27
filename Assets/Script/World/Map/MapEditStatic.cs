using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MapEditStatic : MonoBehaviour
{
    public static MapEditStatic Instance { get; private set; }  
    private List<(Vector3 chunkCoordinate, Vector3 blockCoordinate, float breakCost, float breakThreshold)> blockDataList;
    
    void Start()
    {
        Instance = this;
        blockDataList = new List<(Vector3, Vector3, float, float)>();
    }

    public void BreakBlock(Vector3Int worldPosition, Vector3Int chunkCoordinate, Vector3Int blockCoordinate, float breakValue)
    {
        
        // Check if the coordinates already exist in the list
        var existingBlockData = blockDataList.Find(data => data.chunkCoordinate == chunkCoordinate && data.blockCoordinate == blockCoordinate);

        float breakCost;
        float breakThreshold;
        string blockNameID;
        if (existingBlockData != default)
        {
            // Use existing breakCost and breakThreshold
            breakCost = existingBlockData.breakCost;
            breakThreshold = existingBlockData.breakThreshold;
            blockNameID = BlockStatic.ConvertID(MapLoadStatic.Instance.GetBlockInChunk(chunkCoordinate, blockCoordinate, WorldStatic.Instance));
        }
        else
        {
            // Check if the block is occupied
            blockNameID = BlockStatic.ConvertID(MapLoadStatic.Instance.GetBlockInChunk(chunkCoordinate, blockCoordinate, WorldStatic.Instance));
            if (blockNameID == null)
            {
                return; // Block is not occupied or an error occurred
            }

            // Get the block value
            BlockData targetBlockData = BlockStatic.GetBlock(blockNameID);
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
                Entity.SpawnItem(blockNameID, Lib.AddToVector(worldPosition, 0.5f, 0, 0.5f));
                WorldStatic.Instance.UpdateMap(worldPosition, chunkCoordinate, blockCoordinate, 0);
                blockDataList.Remove(existingBlockData);
            }
            else
            {
                // Update the block data in the list
                blockDataList.Remove(existingBlockData);
                blockDataList.Add((chunkCoordinate, blockCoordinate, breakCost, breakThreshold));
            }
        }
    }
}
