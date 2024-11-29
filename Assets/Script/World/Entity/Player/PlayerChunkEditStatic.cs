using System;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Serialization;

public class PlayerChunkEditStatic : MonoBehaviour
{
    public static PlayerChunkEditStatic Instance { get; private set; }  
    // private float range = 8f;
    public AudioClip SOUNDDIG;
    
    private BoxCollider _boxCollider;
    private GameObject _block;
    private Vector3Int _worldPosition;
    private Vector3 _screenPosition;
    private Vector3Int _chunkCoordinate;
    private Vector3Int _blockCoordinate;
    private int _chunkSize; 
    [HideInInspector] public string _blockStringID = null; 
    [HideInInspector] public ItemData _toolData;

    public int RANGE = 4; 
    public int BLOCKOVERLAYSPEED = 10; 

    private void Awake()
    {
        Instance = this;
        _boxCollider = GetComponent<BoxCollider>();
        _chunkSize = WorldStatic.CHUNK_SIZE; 
    }

    public void HandleTerraformUpdate()
    { 
        if(_block != null)
        { 
            if (_worldPosition != Vector3.down)
            {         
                if (Input.GetMouseButtonDown(1)) //place
                { 
                    if (!WorldStatic.Instance.IsInWorldBounds(_worldPosition)) return;
                    PlayerInventoryStatic.RemoveItem(_blockStringID); 
                    ReplaceBlock(BlockStatic.ConvertID(_blockStringID));
                }
                _block.transform.position = Vector3.Lerp(_block.transform.position, _worldPosition, Time.deltaTime * BLOCKOVERLAYSPEED);
            } else _block.transform.position = Vector3.down;
        } 

        if (_toolData != null)
        {
            if (Input.GetMouseButtonDown(0)) //break
            {
                HandleBlockPosition(true);
                if (!WorldStatic.Instance.IsInWorldBounds(_worldPosition)) return;
                int destroyedBlockID =
                    MapLoadStatic.Instance.GetBlockInChunk(_chunkCoordinate, _blockCoordinate, WorldStatic.Instance);
                if (destroyedBlockID != 0)
                {
                    //occupied check 
                    MapEditStatic.Instance.BreakBlock(_worldPosition, _chunkCoordinate, _blockCoordinate, _toolData.Damage);
                    AudioStatic.PlaySFX(SOUNDDIG);
                }
            }
            else if (Input.GetMouseButtonDown(4)) //break top
            {
                _worldPosition = Lib.AddToVector(Vector3Int.FloorToInt(Game.Player.transform.position), 0, 1, 0);
                if (!WorldStatic.Instance.IsInWorldBounds(_worldPosition)) return;
                HandleChunkCoordinate(_worldPosition);
                int destroyedBlockID =
                    MapLoadStatic.Instance.GetBlockInChunk(_chunkCoordinate, _blockCoordinate, WorldStatic.Instance);
                if (destroyedBlockID != 0)
                {
                    //occupied check
                    MapEditStatic.Instance.BreakBlock(_worldPosition, _chunkCoordinate, _blockCoordinate,  _toolData.Damage);
                    AudioStatic.PlaySFX(SOUNDDIG);
                }
            }
            else if (Input.GetMouseButtonDown(3)) //break under
            {
                _worldPosition = Lib.AddToVector(Vector3Int.FloorToInt(Game.Player.transform.position), 0, -1, 0);
                if (!WorldStatic.Instance.IsInWorldBounds(_worldPosition)) return;
                HandleChunkCoordinate(_worldPosition);
                int destroyedBlockID =
                    MapLoadStatic.Instance.GetBlockInChunk(_chunkCoordinate, _blockCoordinate, WorldStatic.Instance);
                if (destroyedBlockID != 0)
                {
                    //occupied check
                    MapEditStatic.Instance.BreakBlock(_worldPosition, _chunkCoordinate, _blockCoordinate,  _toolData.Damage);
                    AudioStatic.PlaySFX(SOUNDDIG);
                }
            }
        }
 
    }


    void FixedUpdate()
    {    
        if (!string.IsNullOrEmpty(_blockStringID))
        {
            if (_block == null) 
            {
                _block = BlockPreviewStatic.Instance.CreateBlock(_blockStringID);
            }
            else if (_block.name != _blockStringID)
            {
                BlockPreviewStatic.Instance.DeleteBlock();
                _block = BlockPreviewStatic.Instance.CreateBlock(_blockStringID);
            }
            
            HandleBlockPosition();  
        } 
        else
        {
            _block = null;
            BlockPreviewStatic.Instance.DeleteBlock();
        }
         
    }















 
    void HandleBlockPosition(bool isBreak = false)
    {
        HandleWorldCoordinate(isBreak);  
        HandleRange(isBreak);
    }

    private Vector3 _thresholdPoint;
    void HandleWorldCoordinate(bool isBreak)
    {
        float yThreshold = MapCullStatic.Instance._yThreshold + 0.05f;

        _screenPosition = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(_screenPosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            _worldPosition = new Vector3Int();
            Vector3 adjustedPoint;

            if (MapCullStatic.Instance._yCheck)
            {
                // Calculate the position in the ray's direction where y = yThreshold
                float distanceToThreshold = (yThreshold - hitInfo.point.y) / ray.direction.y;
                 _thresholdPoint = hitInfo.point + ray.direction * distanceToThreshold;
                if (isBreak && !WorldStatic.Instance.GetBoolInMap(Vector3Int.FloorToInt(_thresholdPoint) + Vector3Int.down))
                {
                    hitInfo.point = _thresholdPoint;
                }
                else
                {
                    // Cast the ray from the threshold point
                    ray = new Ray(_thresholdPoint, ray.direction);
                    if (!Physics.Raycast(ray, out hitInfo))
                    {
                        return;
                    }
                }
            }
 
            if (isBreak)
            { 
                // Move 0.2 units further in the ray direction
                adjustedPoint = hitInfo.point + ray.direction * 0.1f;
            }
            else
            {
                // Move 0.2 units back from the ray contact point
                adjustedPoint = hitInfo.point - ray.direction * 0.1f;
            }

            _worldPosition.x = Mathf.FloorToInt(adjustedPoint.x);
            _worldPosition.y = Mathf.FloorToInt(adjustedPoint.y);
            _worldPosition.z = Mathf.FloorToInt(adjustedPoint.z);

            // if (_worldPosition.y > WorldStatic.World.Bounds.y - 1) _worldPosition.y = WorldStatic.World.Bounds.y - 1;
            // if (_worldPosition.y < 0) _worldPosition.y = 0;
        }
    }


    Vector3Int currentBlockPosition;
    Vector3Int playerPosition;
    Vector3Int closestBlockPosition;
    Bounds playerBounds;
    Bounds blockBounds;
    void HandleRange(bool isBreak)
    // async void HandleRange(bool isBreak)
    {  
        playerPosition = Vector3Int.FloorToInt(transform.position); 
        if (Mathf.Abs(_worldPosition.x - playerPosition.x) > RANGE ||
            Mathf.Abs(_worldPosition.y - playerPosition.y) > RANGE ||
            Mathf.Abs(_worldPosition.z - playerPosition.z) > RANGE)
        {
            _worldPosition = Vector3Int.down;
                
        }
        HandleChunkCoordinate(_worldPosition);
        // if (isBreak)
        // {
        //     if (Mathf.Abs(_worldPosition.x - playerPosition.x) > RANGE ||
        //         Mathf.Abs(_worldPosition.y - playerPosition.y) > RANGE ||
        //         Mathf.Abs(_worldPosition.z - playerPosition.z) > RANGE)
        //     {
        //         _worldPosition = Vector3Int.down;
        //         
        //     }
        //     HandleChunkCoordinate(_worldPosition);
        // }
        // else
        // {
        //     closestBlockPosition = new Vector3Int(0, -1, 0);
        //     float closestDistance = -1; //closest coord to place block in current search
        //     // Define the rectangle bounds at the player's position 
        //     playerBounds = new Bounds(transform.position + _boxCollider.center, _boxCollider.size);
        //     blockBounds = new Bounds(Lib.AddToVector(Vector3Int.FloorToInt(_worldPosition), 0.5f, 0.5f, 0.5f), Vector3.one); 
        //     bool isEmpty = WorldStatic.Instance.GetBoolInBoolMap(Vector3Int.FloorToInt(_worldPosition));
        //     if (!isEmpty || Mathf.Abs(_worldPosition.x - playerPosition.x) > RANGE ||
        //         Mathf.Abs(_worldPosition.y - playerPosition.y) > RANGE ||
        //         Mathf.Abs(_worldPosition.z - playerPosition.z) > RANGE ||
        //         playerBounds.Intersects(blockBounds))
        //     { 
        //         for (int x = -RANGE; x <= RANGE; x++)
        //         {
        //             for (int y = -1; y <= 1; y++)
        //             {
        //                 for (int z = -RANGE; z <= RANGE; z++)
        //                 {
        //                     currentBlockPosition = playerPosition + new Vector3Int(x, y, z); 
        //
        //                     if (currentBlockPosition.y >= 0 && currentBlockPosition.y < _chunkDepth) //y check
        //                     {
        //                         float distance = Vector3.Distance(_worldPosition, currentBlockPosition); //closest check
        //                         if (distance < closestDistance || closestDistance == -1)
        //                         { 
        //                             if (WorldStatic.Instance.GetBoolInBoolMap(currentBlockPosition)) //occupied check
        //                             { 
        //                                 blockBounds = new Bounds(Lib.AddToVector(currentBlockPosition, 0.5f, 0.5f, 0.5f), Vector3.one);
        //
        //                                 if (!playerBounds.Intersects(blockBounds)) //player stuck check
        //                                 {   
        //                                     closestDistance = distance;
        //                                     closestBlockPosition = currentBlockPosition;
        //                                 }
        //                             }
        //                         }
        //                     }
        //                 }
        //             }
        //         }
        //         _worldPosition = closestBlockPosition; 
        //     }  
        //     HandleChunkCoordinate(_worldPosition); 
        // }  
    }

    void HandleChunkCoordinate(Vector3 coordinate)
    {
        // Calculate chunk coordinates
        _chunkCoordinate = new Vector3Int(
            Mathf.FloorToInt(coordinate.x / _chunkSize) *_chunkSize, 
            Mathf.FloorToInt(coordinate.y / _chunkSize) *_chunkSize,
            Mathf.FloorToInt(coordinate.z / _chunkSize) *_chunkSize
        );

        // Calculate block coordinates within the chunk
        _blockCoordinate = new Vector3Int(
            (int) coordinate.x % _chunkSize,
            (int) coordinate.y % _chunkSize,
            (int) coordinate.z % _chunkSize
        );
 

        if (_blockCoordinate.x < 0) _blockCoordinate.x += _chunkSize;
        if (_blockCoordinate.y < 0) _blockCoordinate.y += _chunkSize;
        if (_blockCoordinate.z < 0) _blockCoordinate.z += _chunkSize;
    }


    void ReplaceBlock(int blockID)
    { 
        AudioStatic.PlaySFX(SOUNDDIG); 
        HandleBlockPosition();  
        _block.transform.position = _worldPosition;
        WorldStatic.Instance.UpdateMap(_worldPosition, _chunkCoordinate, _blockCoordinate, blockID); 
        // Debug.DrawRay(ray.origin, ray.direction * hitInfo.distance, Color.red);
    }
    
    void OnDrawGizmos()
    {
        // Adding 0.5 to each coordinate
    
        // Adjusting position for hit threshold
    
        // Drawing red sphere for world position
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(new Vector3(_worldPosition.x + 0.5f, _worldPosition.y + 0.5f, _worldPosition.z + 0.5f), Vector3.one);
    
        // Drawing blue sphere for hit threshold
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(Vector3Int.FloorToInt(_thresholdPoint) + Vector3Int.down, 0.1f);
    }


 
    // void OnGUI()
    // {
    //     GUIStyle style = new GUIStyle
    //     {
    //         fontSize = 24
    //     };
    //     style.normal.textColor = Color.white;

    //     GUI.Label(new Rect(500, 10, 100, 20), _chunkCoordinate + "chunk block: " + _blockCoordinate + "_worldPosition" + _worldPosition, style);
    // }
}
