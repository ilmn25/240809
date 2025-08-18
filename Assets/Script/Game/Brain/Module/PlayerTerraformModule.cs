using System.Collections.Generic;
using UnityEngine;

public class PlayerTerraformModule : Module
{
    public static List<Vector3Int> Position = new List<Vector3Int>();
    private static GameObject _block;
    private static Vector3 _position;
    private static Vector3 _direction;
    private static Vector3Int _coordinate;

    public static readonly int PreviewSpeed = 10;
    public override void Initialize()
    {
        // Inventory.SlotUpdate += EventSlotUpdate;
        _block = ObjectPool.GetObject("block");
        _block.SetActive(false);
    }

    private static void EventSlotUpdate()
    {
        if (Inventory.CurrentItemData == null)
        {
            _block.SetActive(false);
        }
        else if (Inventory.CurrentItemData.Type == ItemType.Block)
        {
            _block.SetActive(true);
            if (_block.name != Inventory.CurrentItemData.StringID)
            {
                _block.name = Inventory.CurrentItemData.StringID;
                BlockPreview.Set(_block, Inventory.CurrentItemData.StringID);
                _block.transform.localScale = Vector3.one;
            }  
        } 
        else if (Inventory.CurrentItemData.Name == "blueprint")
        {
            _block.SetActive(true); 
            if (_block.name != "overlay")
            {
                _block.name = "overlay";
                BlockPreview.Set(_block, "overlay");
                _block.transform.localScale = Vector3.one * 1.04f;
            } 
        }
        else
        {
            _block.SetActive(false);
        }
    }
    
    public override void Update()
    {
        EventSlotUpdate();
        // if (GUI.Active) return;
  
        if ( Control.Inst.DigUp.KeyDown())
        {
            _coordinate = Vector3Int.FloorToInt(Game.Player.transform.position) + Vector3Int.up;
            if (Position.Contains(_coordinate)) return;
            SpawnBlock();
        }
        else if ( Control.Inst.DigDown.KeyDown())
        {
            _coordinate = Vector3Int.FloorToInt(Game.Player.transform.position) + Vector3Int.down;
            if (Position.Contains(_coordinate)) return;
            SpawnBlock();
        }
        
        if (_block.activeSelf)
        {
            if (_block.name != "overlay")
            {
                _coordinate = OffsetPosition(false, _position, _direction);
                if (_position != Vector3.down)
                    _block.transform.position = Vector3.Lerp(_block.transform.position, _coordinate+ new Vector3(0.5f, 0, 0.5f), Time.deltaTime * PreviewSpeed);
                else
                    _block.transform.position = Vector3.down;
            }
            else
            { 
                if (_position != Vector3.down)
                    _block.transform.position = Vector3.Lerp(_block.transform.position, _coordinate + new Vector3(0.5f, 0, 0.5f), Time.deltaTime * PreviewSpeed);
                else
                    _block.transform.position = Vector3.down;
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
        if (Position.Contains(_coordinate)) return;
        if (!World.IsInWorldBounds(_coordinate)) return; 
        Audio.PlaySFX(Inventory.CurrentItemData.Sfx);
        SpawnBlock();
        
        Inventory.RemoveItem(_block.name);
    }

    public static void SpawnBlock()
    {
        GameObject gameObject = ObjectPool.GetObject("block"); 
        gameObject.transform.position = _coordinate + new Vector3(0.5f, 0, 0.5f);
        
        BlockMachine currentEntityMachine = gameObject.GetComponent<BlockMachine>() ?? gameObject.AddComponent<BlockMachine>();
        EntityStaticLoad.InviteEntity(currentEntityMachine);
        BlockInfo info = (BlockInfo)Entity.CreateInfo("block", _coordinate);
        
        Block block;
        if (_block.name == "overlay")
        { 
            block = Block.GetBlock(World.GetBlock(_coordinate));
            info.operationType = OperationType.Dig;
            info.blockID = 0;
            info.texture = "overlay";
        }
        else
        { 
            block = Block.GetBlock(_block.name); 
            info.operationType = OperationType.Build;
            info.blockID = Block.ConvertID(block.StringID);
            info.texture = block.StringID;
        }
        
        info.Health = block.BreakCost;
        info.threshold = block.BreakThreshold;
        info.SfxHit = "dig_metal";
        info.SfxDestroy = "dig_metal";
         
        currentEntityMachine.Initialize(info);
        Position.Add(_coordinate);
    }
    
    public static void HandleMapBreak()
    {
        _coordinate = OffsetPosition(true, _position, _direction);
        if (Position.Contains(_coordinate)) return;
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
