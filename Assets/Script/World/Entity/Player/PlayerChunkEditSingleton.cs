using System;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Serialization;

public class PlayerChunkEditSingleton : MonoBehaviour
{
    public static PlayerChunkEditSingleton Instance { get; private set; }  
    
    private GameObject _block;
    private Vector3Int _chunkCoordinate;
    private Vector3Int _blockCoordinate;
    [HideInInspector] public string _blockStringID = null; 
    [HideInInspector] public ItemData _toolData;

    public int BLOCKOVERLAYSPEED = 10; 

    private void Awake()
    {
        Instance = this; 
    }
 

    private Vector3 _position;
    private Vector3 _direction;
    private Vector3Int _coordinate;
    public void HandlePositionInfo(Vector3 position, Vector3 direction)
    {
        _position = position;
        _direction = direction;
        if (_block)
        {
            if (!string.IsNullOrEmpty(_blockStringID))
            {
                _coordinate = OffsetPosition(false, _position, _direction);
                if (position != Vector3.down)
                    _block.transform.position = Vector3.Lerp(_block.transform.position, _coordinate, Time.deltaTime * BLOCKOVERLAYSPEED);
                else
                    _block.transform.position = Vector3.down;
            }
            else
            {
                _coordinate = OffsetPosition(true, _position, _direction);
                if (position != Vector3.down)
                    _block.transform.position = Vector3.Lerp(_block.transform.position, _coordinate - new Vector3(0.02f, 0.02f, 0.02f), Time.deltaTime * BLOCKOVERLAYSPEED);
                else
                    _block.transform.position = Vector3.down;
            } 
        }
    }

    public void HandleMapPlace()
    {
        if (Game.GUIBusy) return;
        
        if (!string.IsNullOrEmpty(_blockStringID)) //place
        {
            _coordinate = OffsetPosition(false, _position, _direction);
            
            if (!WorldSingleton.Instance.IsInWorldBounds(_coordinate)) return;
            
            InventorySingleton.RemoveItem(_blockStringID);
            
            if (MapLoadSingleton.Instance.GetBlockInChunk(_coordinate) == 0)
                WorldSingleton.Instance.UpdateMap(_coordinate, BlockSingleton.ConvertID(_blockStringID));
        }
    }

    public void HandleMapBreak()
    {
        _coordinate = OffsetPosition(true, _position, _direction);
        BreakBlock();
    } 

    void BreakBlock()
    { 
        if (!WorldSingleton.Instance.IsInWorldBounds(_coordinate)) return; 
        if (MapLoadSingleton.Instance.GetBlockInChunk(_coordinate) != 0)
            MapEditSingleton.Instance.BreakBlock(_coordinate,  _toolData?.Damage ?? 1); 
    }
    
    public void HandleChunkEditInput()
    {   
        if (Game.GUIBusy) return;
  
        if (Input.GetMouseButtonDown(4)) //break top
        {
            _coordinate = Vector3Int.FloorToInt(Game.Player.transform.position) + Vector3Int.up;
            BreakBlock();
        }
        else if (Input.GetMouseButtonDown(3)) //break under
        {
            _coordinate = Vector3Int.FloorToInt(Game.Player.transform.position) + Vector3Int.down;
            BreakBlock();
        }
    }
    void FixedUpdate()
    { 
        if (!_block)
        {
            _block = BlockPreviewSingleton.Instance.CreateBlock("overlay");
            _block.transform.localScale = Vector3.one * 1.04f;
        }
        else if (!string.IsNullOrEmpty(_blockStringID))
        { 
            if (_block.name != _blockStringID)
            {
                BlockPreviewSingleton.Instance.DeleteBlock();
                _block = BlockPreviewSingleton.Instance.CreateBlock(_blockStringID);
            } 
        } 
        else if (_block.name != "overlay")
        {
            BlockPreviewSingleton.Instance.DeleteBlock();
            _block = BlockPreviewSingleton.Instance.CreateBlock("overlay");
            _block.transform.localScale = Vector3.one * 1.04f;
        }
    }

    public Vector3Int OffsetPosition(bool isBreak, Vector3 position, Vector3 direction)
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
