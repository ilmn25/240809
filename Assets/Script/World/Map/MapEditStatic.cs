using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MapEditStatic : MonoBehaviour
{
    private WorldStatic _worldStatic;
    private ItemLoadStatic _itemLoadStatic;
    private BlockStatic _blockStatic;
    private List<(Vector3 chunkCoordinate, Vector3 blockCoordinate, float breakCost, float breakThreshold)> blockDataList;
    
    void Start()
    {
        _worldStatic = GameObject.Find("world_system").GetComponent<WorldStatic>();
        _itemLoadStatic = GameObject.Find("entity_system").GetComponent<ItemLoadStatic>();
        _blockStatic = GetComponent<BlockStatic>();
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
            blockID = BlockStatic.ConvertID(_worldStatic.MapLoadStatic.GetBlockInChunk(chunkCoordinate, blockCoordinate, _worldStatic));
        }
        else
        {
            // Check if the block is occupied
            blockID = BlockStatic.ConvertID(_worldStatic.MapLoadStatic.GetBlockInChunk(chunkCoordinate, blockCoordinate, _worldStatic));
            if (blockID == null)
            {
                return; // Block is not occupied or an error occurred
            }

            // Get the block value
            BlockData targetBlockData = BlockStatic.GetBlock(blockID);
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
                _itemLoadStatic.SpawnItem(ItemLoadStatic.GetEntityData(_itemLoadStatic.GetItemNameID(blockID), 
                    Lib.AddToVector(worldPosition, 0.5f, 0.7f, 0.5f), EntityType.Item));
                _worldStatic.UpdateMap(chunkCoordinate, blockCoordinate, 0);
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
