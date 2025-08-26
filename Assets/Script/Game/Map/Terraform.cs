using System.Collections.Generic;
using UnityEngine;

public class Terraform
{
    public static List<Vector3Int> PendingBlocks = new ();
    public static GameObject Block;
    private static Vector3 _position;
    private static Vector3 _direction;
    private static Vector3Int _coordinate;

    public static readonly int PreviewSpeed = 10; 

    public static void Initialize()
    {
        Block = ObjectPool.GetObject(ID.BlockPrefab);
        Block.SetActive(false); 
    }
    
    private static void SlotUpdate()
    {
        if (Inventory.CurrentItemData == null)
        {
            Block.SetActive(false);
        }
        else if (Inventory.CurrentItemData.Type == ItemType.Block)
        {
            Block.SetActive(true);
            if (Block.name != Inventory.CurrentItemData.ID.ToString())
            {
                Block.name = Inventory.CurrentItemData.ID.ToString();
                BlockPreview.Set(Block, Inventory.CurrentItemData.ID);
                Block.transform.localScale = Vector3.one;
            }  
        } 
        else if (Inventory.CurrentItemData.Name == "blueprint")
        {
            Block.SetActive(true); 
            if (Block.name != "overlay")
            {
                Block.name = "overlay";
                BlockPreview.Set(Block, ID.OverlayBlock);
                Block.transform.localScale = Vector3.one * 1.04f;
            } 
        }
        else
        {
            Block.SetActive(false);
        }
    }
    
    public static void Update()
    {
        SlotUpdate();
        // if (GUI.Active) return;
  
        if ( Control.Inst.DigUp.KeyDown())
        {
            _coordinate = Vector3Int.FloorToInt(Game.Player.transform.position) + Vector3Int.up;
            if (PendingBlocks.Contains(_coordinate)) return;
            SpawnBlock();
        }
        else if ( Control.Inst.DigDown.KeyDown())
        {
            _coordinate = Vector3Int.FloorToInt(Game.Player.transform.position) + Vector3Int.down;
            if (PendingBlocks.Contains(_coordinate)) return;
            SpawnBlock();
        }
        
        if (Block.activeSelf)
        {
            if (Block.name != "overlay")
            {
                _coordinate = OffsetPosition(false, _position, _direction);
                if (_position != Vector3.down)
                    Block.transform.position = Vector3.Lerp(Block.transform.position, _coordinate+ new Vector3(0.5f, 0, 0.5f), Time.deltaTime * PreviewSpeed);
                else
                    Block.transform.position = Vector3.down;
            }
            else
            { 
                if (_position != Vector3.down)
                    Block.transform.position = Vector3.Lerp(Block.transform.position, _coordinate + new Vector3(0.5f, 0, 0.5f), Time.deltaTime * PreviewSpeed);
                else
                    Block.transform.position = Vector3.down;
            } 
        }
    }
 
    public static void HandlePositionInfo(Vector3 position, Vector3 direction, bool isBreak)
    {
        _position = position;
        _direction = direction; 
        _coordinate = OffsetPosition(isBreak, _position, _direction);
    }

    public static void HandleMapPlace()
    {
        _coordinate = OffsetPosition(false, _position, _direction);
        if (PendingBlocks.Contains(_coordinate)) return;
        if (!World.IsInWorldBounds(_coordinate)) return; 
        Audio.PlaySFX(Inventory.CurrentItemData.Sfx);
        SpawnBlock();
        
        Game.PlayerInfo.storage.RemoveItem(Game.PlayerInfo.Equipment.ID);
    }

    public static void SpawnBlock()
    {
        if (Game.BuildMode)
        {
            if (Block.name == "overlay")
                World.SetBlock(_coordinate);
            else
            {
                Game.PlayerInfo.storage.CreateAndAddItem(Game.PlayerInfo.Equipment.ID);
                World.SetBlock(_coordinate, global::Block.ConvertID(Game.PlayerInfo.Equipment.ID)); 
            }
            return;
        }
        
        StructureInfo info;
        Block block;
        if (Block.name == "overlay")
        { 
            block = global::Block.GetBlock(World.GetBlock(_coordinate));
            info = (BreakBlockInfo)Entity.Spawn(ID.BreakBlock, _coordinate);
            info.operationType = OperationType.Mining;
            info.Loot = block.StringID; 
        }
        else
        { 
            block = global::Block.GetBlock(Game.PlayerInfo.Equipment.ID);
            info = (BlockInfo)Entity.Spawn(ID.Block, _coordinate);
            info.operationType = OperationType.Building;
            ((BlockInfo)info).blockID = block.StringID;
            BlockPreview.Set(info.Machine.gameObject, block.StringID);
        }
        
        info.Health = block.BreakCost;
        info.threshold = block.BreakThreshold;
        info.SfxHit = SfxID.HitMetal;
        info.SfxDestroy = SfxID.HitMetal;
         
        PendingBlocks.Add(_coordinate);
        PlayerTask.Pending.Add(info);
    }
    
    public static void HandleMapBreak()
    {
        _coordinate = OffsetPosition(true, _position, _direction);
        if (PendingBlocks.Contains(_coordinate)) return; 
        SpawnBlock();
    }
    
    public static Vector3Int OffsetPosition(bool isBreak, Vector3 position, Vector3 direction)
    {
        Vector3 adjustedPoint;
        if (isBreak)
        { 
            adjustedPoint = position + direction * 0.1f;
        }
        else
        {
            adjustedPoint = position - direction * 0.1f;
        }
 
        return Vector3Int.FloorToInt(adjustedPoint);
    } 
}
