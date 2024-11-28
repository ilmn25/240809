using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering; 

public class MapCullStatic : MonoBehaviour
{ 
    public static MapCullStatic Instance { get; private set; }  
    public static event Action _signalUpdateSpriteYCull;
 
    [HideInInspector] 
    public int _cullSyncFrame = 0;  
    private int _yThresholdPrevious = 0;  
    private bool _yCheckPrevious = false;   
    private Vector3 _chunkPositionPrevious;  
    [HideInInspector] 
    public bool _yCheck = false;   
    [HideInInspector] 
    public int _yThreshold = 0; 
    private int _visionHeight = 1;
    private enum CullMode { On, Off, Both}
    private CullMode _currentCullMode = CullMode.Both;
    
    private GameObject _lightIndoor;
    private GameObject _lightSelf;

    private GameObject _camera;
    private LayerMask _collisionLayer;
    private Volume _volume;
 
    Vector3 forwardDirection;
    Vector3 backwardDirection;
    Vector3 leftDirection;
    Vector3 rightDirection;

    Ray rayForward;
    Ray rayBackward;
    Ray rayLeft;
    Ray rayRight;
    Ray rayToCamera;
    Vector3 playerPosition;

    private float ANGLE_OFFSET = 12; // Angle in degrees 

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        forwardDirection = Quaternion.Euler(ANGLE_OFFSET, 5, 0) * Vector3.up;
        backwardDirection = Quaternion.Euler(-ANGLE_OFFSET, 5, 0) * Vector3.up;
        leftDirection = Quaternion.Euler(0, 5, -ANGLE_OFFSET) * Vector3.up;
        rightDirection = Quaternion.Euler(0, 5, ANGLE_OFFSET) * Vector3.up;
 
        _chunkPositionPrevious = WorldStatic._playerChunkPos;

        _collisionLayer = LayerMask.GetMask("Collision");
        _lightIndoor = Game.Player.transform.Find("light_indoor").gameObject;
        _lightSelf = Game.Player.transform.Find("light_self").gameObject;   
        
        _camera = Game.Camera.gameObject; 
        _volume = _camera.GetComponent<Volume>();
        
        HandleLight(false); 
    }

    void Update()
    {
        HandleInput();

        if (PlayerMovementStatic.Instance._verticalVelocity == 0)
        {
            HandleObstructionCheck();
            HandleCheck(); 
        }
    }
 
 
    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Z)) {
            switch (_currentCullMode)
            {
                case CullMode.On:
                    _currentCullMode = CullMode.Off;
                    break;
                case CullMode.Off:
                    _currentCullMode = CullMode.Both;
                    break;
                case CullMode.Both:
                    _currentCullMode = CullMode.On;
                    break;
            }
        }
        
        // float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        // if (Input.GetKey(KeyCode.LeftShift) && scrollInput != 0)
        // {
        //     _visionHeight += (scrollInput > 0) ? 1 : -1;
        //     if (_visionHeight > 3) _visionHeight = 3;
        //     if (_visionHeight < 0) _visionHeight = 0;
        // } 
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            _visionHeight++;
            if (_visionHeight == 4) _visionHeight = 1;
        }
    }

    void HandleCheck()
    {
        if (Time.frameCount > _cullSyncFrame + 1)
        {
            if (!_yCheckPrevious && _yCheck) // enter
            {
                HandleLight(true);
                UpdateYCullDelayed(_yCheck);
            }
            else if (_yCheckPrevious && !_yCheck) // exit
            {
                HandleLight(false);
                UpdateYCullDelayed(_yCheck);
            }
            else if (_yCheck)   
            {
                if (WorldStatic._playerChunkPos != _chunkPositionPrevious)
                { 
                    UpdateYCull();
                    _chunkPositionPrevious = WorldStatic._playerChunkPos;
                }
                else if (_yThresholdPrevious != _yThreshold)
                {
                    UpdateYCull();
                }
            }

            _yCheckPrevious = _yCheck;
            _yThresholdPrevious = _yThreshold;  
        }
    }

    public int CULL_SYNC_DELAY = 3; // Angle in degrees 
    private bool _delayBuffer;
    void UpdateYCull()
    {  
        _signalUpdateSpriteYCull?.Invoke();   
        _cullSyncFrame = Time.frameCount + CULL_SYNC_DELAY;
        foreach (var kvp in MapLoadStatic.Instance._activeChunks) 
        { 
            kvp.Value.CullMeshAsync();
        }  
    }  

    async void UpdateYCullDelayed(bool yCheckPrevious)
    { 
        if (_delayBuffer) return;
        _delayBuffer = true;

        await Task.Delay(120);
        if (_yCheck == yCheckPrevious)
        { 
            UpdateYCull();
        } 
        _delayBuffer = false;
    }
  
 
    void HandleLight(bool flag)
    {
        _lightIndoor.SetActive(flag);
        _lightSelf.SetActive(flag); 
        _volume.profile = flag? Resources.Load<VolumeProfile>("shader/preset/volume_indoor") : Resources.Load<VolumeProfile>("shader/preset/volume_outdoor"); 
    }
 

    void HandleObstructionCheck()
    {
        if (_currentCullMode == CullMode.Off)
        {
            _yCheck = false;
            return;
        }
        
        playerPosition = Game.Player.transform.position;
        if (_currentCullMode == CullMode.On)
        {
            _yCheck = true;
            _yThreshold = (int)playerPosition.y + _visionHeight;
            return;
        }  
    
        rayToCamera = new Ray(playerPosition + Vector3.up/2,  _camera.transform.position - (playerPosition + Vector3.up/2));
        if (Physics.Raycast(rayToCamera, out _, 50, _collisionLayer))
        {
            _yCheck = true;
            _yThreshold = (int)playerPosition.y + _visionHeight;
            return;
        }
        
        rayForward = new Ray(playerPosition, forwardDirection);
        rayBackward = new Ray(playerPosition, backwardDirection);
        rayLeft = new Ray(playerPosition, leftDirection);
        rayRight = new Ray(playerPosition, rightDirection);
        if (Physics.Raycast(rayForward, out _, 50, _collisionLayer) &&
            Physics.Raycast(rayBackward, out _, 50, _collisionLayer) &&
            Physics.Raycast(rayLeft, out _, 50, _collisionLayer) &&
            Physics.Raycast(rayRight, out _, 50, _collisionLayer)) {
            _yCheck = true;
            _yThreshold = (int)playerPosition.y + _visionHeight;
        }
     
        _yCheck = false;
        _yThreshold = 0;
    }

    // void OnDrawGizmos()
    // {
    //     Vector3 playerPosition = Game.Player.transform.position;
    //     Vector3 cameraPosition = _camera.transform.position;
    //     Ray rayToCamera = new Ray(playerPosition + Vector3.up/2, cameraPosition - (playerPosition + Vector3.up/2));
    //
    //     Gizmos.color = Color.red;
    //     Gizmos.DrawLine(rayToCamera.origin, rayToCamera.origin + rayToCamera.direction * 100);
    // }

    //
    //
    // void HandleObstructionCheck()
    // {
    //     if (_currentCullMode == CullMode.Off)
    //     {
    //         _yCheck = false;
    //         return;
    //     }
    //     
    //     playerPosition = Game.Player.transform.position;
    //     
    //     if (_currentCullMode == CullMode.On)
    //     {
    //         _yCheck = true;
    //         _yThreshold = (int)playerPosition.y + _visionHeight;
    //         return;
    //     }  
    //
    //     rayToCamera = new Ray(playerPosition + Vector3.up * 0.5f, _camera.transform.position - playerPosition);
    //     if (Physics.Raycast(rayToCamera, out _, 50, _collisionLayer))
    //     {
    //         
    //         _yThreshold = (int)playerPosition.y + _visionHeight;
    //         _yCheck = true;  
    //     }
    //     else
    //     {
    //         _yCheck = false;
    //         _yThreshold = 0; 
    //     }
    //      
    // }
 

            
    public void ForceRevertMesh() // for when player fall into void
    {
        _yCheck = false;
        _yThreshold = 0;
        HandleLight(false); 
        UpdateYCull(); 
    }

}
