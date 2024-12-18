using System;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Serialization;

public class PlayerTerraform
{
    private static GameObject _block;
    public static string BlockStringID = null; 
    public static ItemData ToolData;
    
    private static Vector3 _position;
    private static Vector3 _direction;
    private static Vector3Int _coordinate;

    public static readonly int PreviewSpeed = 10; 

    public static void Update()
    { 
        if (!_block)
        {
            _block = BlockPreview.Create("overlay");
            _block.transform.localScale = Vector3.one * 1.04f;
        }
        else if (!string.IsNullOrEmpty(BlockStringID))
        { 
            if (_block.name != BlockStringID)
            {
                BlockPreview.Delete();
                _block = BlockPreview.Create(BlockStringID);
            } 
        } 
        else if (_block.name != "overlay")
        {
            BlockPreview.Delete();
            _block = BlockPreview.Create("overlay");
            _block.transform.localScale = Vector3.one * 1.04f;
        }
        
        
        if (GUI.Active) return;
  
        if ( Control.control.DigUp.KeyDown())
        {
            _coordinate = Vector3Int.FloorToInt(Game.Player.transform.position) + Vector3Int.up;
            BreakBlock();
        }
        else if ( Control.control.DigDown.KeyDown())
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
            if (!string.IsNullOrEmpty(BlockStringID))
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
        if (GUI.Active) return;
        
        if (!string.IsNullOrEmpty(BlockStringID)) //place
        {
            _coordinate = OffsetPosition(false, _position, _direction);
            
            if (!World.IsInWorldBounds(_coordinate)) return;
            
            Inventory.RemoveItem(BlockStringID);
            
            Audio.PlaySFX(Game.DigSound);
            if (World.GetBlock(_coordinate) == 0)
                World.SetBlock(_coordinate, Block.ConvertID(BlockStringID));
        }
    }

    public static void HandleMapBreak()
    {
        _coordinate = OffsetPosition(true, _position, _direction);
        BreakBlock();
    }

    public static void BreakBlock()
    {  
        MapEdit.BreakBlock(_coordinate,  ToolData?.Damage ?? 1); 
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
