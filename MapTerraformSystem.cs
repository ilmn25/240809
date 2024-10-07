using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MapTerraformSystem : MonoBehaviour
{
    private ChunkSystem _chunkSystem;
    private ItemLoadSystem _itemLoadSystem;
    private BlockSystem _blockSystem;
    private List<(Vector3 chunkCoordinate, Vector3 blockCoordinate, float breakCost, float breakThreshold)> blockDataList;
    
    void Start()
    {
        _chunkSystem = GameObject.Find("world_system").GetComponent<ChunkSystem>();
        _itemLoadSystem = GameObject.Find("entity_system").GetComponent<ItemLoadSystem>();
        _blockSystem = GetComponent<BlockSystem>();
        blockDataList = new List<(Vector3, Vector3, float, float)>();
    }

    public void BreakBlock(Vector3 worldPosition, Vector3Int chunkCoordinate, Vector3Int blockCoordinate, float breakValue)
    {
        
        // Check if the coordinates already exist in the list
        var existingBlockData = blockDataList.Find(data => data.chunkCoordinate == chunkCoordinate && data.blockCoordinate == blockCoordinate);

        float breakCost;
        float breakThreshold;
        string blockID;
        if (existingBlockData != default)
        {
            // Use existing breakCost and breakThreshold
            breakCost = existingBlockData.breakCost;
            breakThreshold = existingBlockData.breakThreshold;
            blockID = BlockSystem.GetStringIDByID(_chunkSystem.GetBlockInChunk(chunkCoordinate, blockCoordinate));
        }
        else
        {
            // Check if the block is occupied
            blockID = BlockSystem.GetStringIDByID(_chunkSystem.GetBlockInChunk(chunkCoordinate, blockCoordinate));
            if (blockID == null)
            {
                return; // Block is not occupied or an error occurred
            }

            // Get the block value
            Block targetBlock = BlockSystem.GetBlockByStringID(blockID);
            breakCost = targetBlock.BreakCost;
            breakThreshold = targetBlock.BreakThreshold;
            
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
                _itemLoadSystem.SpawnItem(_itemLoadSystem.GetItemNameID(blockID), CustomLibrary.AddToVector(worldPosition, 0.5f, 0.7f, 0.5f));  
                _chunkSystem.UpdateBlock(chunkCoordinate, blockCoordinate, 0);
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
