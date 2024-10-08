using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;

public class PlayerTerraformSystem : MonoBehaviour
{
    // private float range = 8f;
    public AudioClip SOUNDDIG;
    private ChunkSystem _chunkSystem;
    private MapTerraformSystem _mapTerraformSystem;
    private MapYCullSystem _mapYCullSystem;
    private BlockPreviewSystem _blockPreviewSystem;
    private PlayerDataSystem _playerDataSystem;
    private ItemLoadSystem _itemLoadSystem;
    
    BoxCollider _boxCollider;
    private GameObject _block;
    private GameObject _player;
    private Vector3 _worldPosition;
    private Vector3 _screenPosition;
    private Vector3Int _chunkCoordinate;
    private Vector3Int _blockCoordinate;
    private int _chunkSize;
    private int _chunkDepth; 
    [HideInInspector] public string _blockID; 

    public int RANGE = 4; 
    public int BLOCKOVERLAYSPEED = 10; 

    private void Awake()
    { 
        _player = GameObject.Find("player");
        _boxCollider = GetComponent<BoxCollider>();
        GameObject userSystem = GameObject.Find("user_system");
        GameObject mapSystem = GameObject.Find("map_system");
        _chunkSystem = GameObject.Find("world_system").GetComponent<ChunkSystem>();
        _itemLoadSystem = GameObject.Find("entity_system").GetComponent<ItemLoadSystem>();
        _mapTerraformSystem = mapSystem.GetComponent<MapTerraformSystem>();
        _mapYCullSystem = mapSystem.GetComponent<MapYCullSystem>();
        _blockPreviewSystem = userSystem.GetComponent<BlockPreviewSystem>(); 
        _playerDataSystem = userSystem.GetComponent<PlayerDataSystem>(); 
        _chunkSize = ChunkSystem.CHUNKSIZE;
        _chunkDepth = ChunkSystem.CHUNKDEPTH; 
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) //break
        {  
            HandleBlockPosition(true);
            int destroyedBlockID = _chunkSystem.GetBlockInChunk(_chunkCoordinate, _blockCoordinate);
            if (destroyedBlockID != 0) { //occupied check 
                _mapTerraformSystem.BreakBlock(_worldPosition, _chunkCoordinate, _blockCoordinate, 1);
                AudioSystem.PlaySFX(SOUNDDIG);
            }   
        }
        else if (Input.GetMouseButtonDown(1) && !string.IsNullOrEmpty(_blockID)) //place
        { 
            _playerDataSystem.RemoveItem(_itemLoadSystem.GetItemNameID(_blockID)); 
            ReplaceBlock(BlockSystem.GetIDByStringID(_blockID));
        }
        else if (Input.GetKeyDown(KeyCode.X)) //break top
        {  
            _worldPosition = CustomLibrary.AddToVector(Vector3Int.FloorToInt(_player.transform.position), 0f, 2f, 0f);
            HandleChunkCoordinate(_worldPosition);
            int destroyedBlockID = _chunkSystem.GetBlockInChunk(_chunkCoordinate, _blockCoordinate);
            if (destroyedBlockID != 0) { //occupied check
                _mapTerraformSystem.BreakBlock(_worldPosition, _chunkCoordinate, _blockCoordinate, 1);
                AudioSystem.PlaySFX(SOUNDDIG);
            }   
        }
        else if (Input.GetKeyDown(KeyCode.C)) //break under
        {  
            _worldPosition = CustomLibrary.AddToVector(Vector3Int.FloorToInt(_player.transform.position), 0f, -1f, 0f);
            HandleChunkCoordinate(_worldPosition);
            int destroyedBlockID = _chunkSystem.GetBlockInChunk(_chunkCoordinate, _blockCoordinate);
            if (destroyedBlockID != 0) { //occupied check
                _mapTerraformSystem.BreakBlock(_worldPosition, _chunkCoordinate, _blockCoordinate, 1);
                AudioSystem.PlaySFX(SOUNDDIG);
            }   
        }

        if (_block != null && _worldPosition != new Vector3Int(0, -1, 0))
        {         
            _block.transform.position = Vector3.Lerp(_block.transform.position, _worldPosition, Time.deltaTime * BLOCKOVERLAYSPEED);
        } 
    }


    void FixedUpdate()
    {    
        if (_blockID != null)
        {        
            if (_block == null) 
            {
                _block = _blockPreviewSystem.CreateBlock(_blockID);
            }
            else if (_block.name != _blockID)
            {
                _blockPreviewSystem.DeleteBlock();
                _block = _blockPreviewSystem.CreateBlock(_blockID);
            }
            
            HandleBlockPosition();  
        } 
        else
        {
            _block = null;
            _blockPreviewSystem.DeleteBlock();
        }
    }















 
    void HandleBlockPosition(bool isBreak = false)
    {
        HandleWorldCoordinate(isBreak);  
        HandleRange(isBreak);
    }


    void HandleWorldCoordinate(bool isBreak)
    {
        float yThreshold = _mapYCullSystem._yThreshold + 0.05f;

        _screenPosition = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(_screenPosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            _worldPosition = new Vector3Int();
            Vector3 adjustedPoint;

            if (_mapYCullSystem._yCheck)
            {
                // Calculate the position in the ray's direction where y = yThreshold
                float distanceToThreshold = (yThreshold - hitInfo.point.y) / ray.direction.y;
                Vector3 thresholdPoint = hitInfo.point + ray.direction * distanceToThreshold;

                // Cast the ray from the threshold point
                ray = new Ray(thresholdPoint, ray.direction);
                if (!Physics.Raycast(ray, out hitInfo))
                {
                    return; // Exit if the ray doesn't hit anything
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

            if (_worldPosition.y > _chunkDepth - 1) _worldPosition.y = _chunkDepth - 1;
            if (_worldPosition.y < 0) _worldPosition.y = 0;
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
        if (isBreak)
        {
            if (Mathf.Abs(_worldPosition.x - playerPosition.x) > RANGE ||
                Mathf.Abs(_worldPosition.y - playerPosition.y) > RANGE ||
                Mathf.Abs(_worldPosition.z - playerPosition.z) > RANGE)
            {
                _worldPosition = Vector3.down;
                
            }
            HandleChunkCoordinate(_worldPosition);
        }
        else
        {
            closestBlockPosition = new Vector3Int(0, -1, 0);
            float closestDistance = -1; //closest coord to place block in current search
            // Define the rectangle bounds at the player's position 
            playerBounds = new Bounds(transform.position + _boxCollider.center, _boxCollider.size);
            blockBounds = new Bounds(CustomLibrary.AddToVector(Vector3Int.FloorToInt(_worldPosition), 0.5f, 0.5f, 0.5f), Vector3.one); 
            bool isEmpty = _chunkSystem.GetBoolInBoolMap(Vector3Int.FloorToInt(_worldPosition));
            if (!isEmpty || Mathf.Abs(_worldPosition.x - playerPosition.x) > RANGE ||
                Mathf.Abs(_worldPosition.y - playerPosition.y) > RANGE ||
                Mathf.Abs(_worldPosition.z - playerPosition.z) > RANGE ||
                playerBounds.Intersects(blockBounds))
            { 
                for (int x = -RANGE; x <= RANGE; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        for (int z = -RANGE; z <= RANGE; z++)
                        {
                            currentBlockPosition = playerPosition + new Vector3Int(x, y, z); 

                            if (currentBlockPosition.y >= 0 && currentBlockPosition.y < _chunkDepth) //y check
                            {
                                float distance = Vector3.Distance(_worldPosition, currentBlockPosition); //closest check
                                if (distance < closestDistance || closestDistance == -1)
                                { 
                                    if (_chunkSystem.GetBoolInBoolMap(currentBlockPosition)) //occupied check
                                    { 
                                        blockBounds = new Bounds(CustomLibrary.AddToVector(currentBlockPosition, 0.5f, 0.5f, 0.5f), Vector3.one);

                                        if (!playerBounds.Intersects(blockBounds)) //player stuck check
                                        {   
                                            closestDistance = distance;
                                            closestBlockPosition = currentBlockPosition;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                _worldPosition = closestBlockPosition; 
            }  
            HandleChunkCoordinate(_worldPosition); 
        }  
    }

    void HandleChunkCoordinate(Vector3 coordinate)
    {
        // Calculate chunk coordinates
        _chunkCoordinate = new Vector3Int(
            Mathf.FloorToInt(coordinate.x / _chunkSize) *_chunkSize, 0,
            Mathf.FloorToInt(coordinate.z / _chunkSize) *_chunkSize
        );

        // Calculate block coordinates within the chunk
        _blockCoordinate = new Vector3Int(
            (int) coordinate.x % _chunkSize,
            (int) coordinate.y,
            (int) coordinate.z % _chunkSize
        );
 

        if (_blockCoordinate.x < 0) _blockCoordinate.x += _chunkSize;
        if (_blockCoordinate.z < 0) _blockCoordinate.z += _chunkSize;
    }


    void ReplaceBlock(int blockID)
    { 
        AudioSystem.PlaySFX(SOUNDDIG); 
        HandleBlockPosition();  
        _block.transform.position = _worldPosition;
        _chunkSystem.UpdateBlock(_chunkCoordinate, _blockCoordinate, blockID); 
        // Debug.DrawRay(ray.origin, ray.direction * hitInfo.distance, Color.red);
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
