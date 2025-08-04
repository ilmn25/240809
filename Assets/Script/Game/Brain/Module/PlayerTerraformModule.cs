using UnityEngine;

public class PlayerTerraformModule : Module
{
    private static GameObject _block;
    private static Vector3 _position;
    private static Vector3 _direction;
    private static Vector3Int _coordinate;

    public static readonly int PreviewSpeed = 10;
    public override void Initialize()
    {
        Inventory.SlotUpdate += EventSlotUpdate;
    }

    private static void EventSlotUpdate()
    {
        if (Inventory.CurrentItemData == null)
        {
            if (_block)
                BlockPreview.Delete();
        }
        else if (Inventory.CurrentItemData.Type == ItemType.Block)
        {
            if (!_block)
            {
                _block = BlockPreview.Create(Inventory.CurrentItemData.StringID);
            }
            else if (_block.name != Inventory.CurrentItemData.StringID)
            {
                BlockPreview.Delete();
                _block = BlockPreview.Create(Inventory.CurrentItemData.StringID);
            }  
        } 
        else if (Inventory.CurrentItemData.MiningPower != 0)
        {
            if (!_block)
            {
                _block = BlockPreview.Create("overlay");
                _block.transform.localScale = Vector3.one * 1.04f;
            }
            else if (_block.name != "overlay")
            {
                BlockPreview.Delete(); 
                _block = BlockPreview.Create("overlay");
                _block.transform.localScale = Vector3.one * 1.04f;
            } 
        }
    }
    
    public override void Update()
    {  
        if (GUI.Active) return;
  
        if ( Control.Inst.DigUp.KeyDown())
        {
            _coordinate = Vector3Int.FloorToInt(Game.Player.transform.position) + Vector3Int.up;
            BreakBlock();
        }
        else if ( Control.Inst.DigDown.KeyDown())
        {
            _coordinate = Vector3Int.FloorToInt(Game.Player.transform.position) + Vector3Int.down;
            BreakBlock();
        }
    }
 
    public static void HandlePositionInfo(Vector3 position, Vector3 direction)
    {
        _position = position;
        _direction = direction;
        if (_block)
        {
            if (_block.name != "overlay")
            {
                _coordinate = OffsetPosition(false, _position, _direction);
                if (position != Vector3.down)
                    _block.transform.position = Vector3.Lerp(_block.transform.position, _coordinate, Time.deltaTime * PreviewSpeed);
                else
                    _block.transform.position = Vector3.down;
            }
            else
            {
                _coordinate = OffsetPosition(true, _position, _direction);
                if (position != Vector3.down)
                    _block.transform.position = Vector3.Lerp(_block.transform.position, _coordinate - new Vector3(0.02f, 0.02f, 0.02f), Time.deltaTime * PreviewSpeed);
                else
                    _block.transform.position = Vector3.down;
            } 
        }
    }

    public static void HandleMapPlace()
    {
        _coordinate = OffsetPosition(false, _position, _direction);
        
        if (!World.IsInWorldBounds(_coordinate)) return; 
        
        Audio.PlaySFX("dig_sand");
        // if (World.GetBlock(_coordinate) == 0)
        World.SetBlock(_coordinate, Block.ConvertID(_block.name));
        Inventory.RemoveItem(_block.name);
    }

    public static void HandleMapBreak()
    {
        _coordinate = OffsetPosition(true, _position, _direction);
        BreakBlock();
    }

    public static void BreakBlock()
    {
        int miningPower = 0;
        if (Inventory.CurrentItemData == null || Inventory.CurrentItemData.Type == ItemType.Block) miningPower = 1;
        else if (Inventory.CurrentItemData.Type == ItemType.Tool ) miningPower = Inventory.CurrentItemData.MiningPower;
        MapEdit.BreakBlock(_coordinate, miningPower);
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

        Vector3Int coordinate = new Vector3Int();
        coordinate.x = Mathf.FloorToInt(adjustedPoint.x);
        coordinate.y = Mathf.FloorToInt(adjustedPoint.y);
        coordinate.z = Mathf.FloorToInt(adjustedPoint.z);
        return coordinate;
    }
}
