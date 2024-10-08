using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering; 

public class MapYCullSystem : MonoBehaviour
{ 
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
    [HideInInspector] 

    private int _visionHeight = 2;
    private bool _forceCull = false;  
    
    private GameObject _player;
    private GameObject _lightIndoor;
    private GameObject _lightSelf;

    private PlayerMovementSystem _playerMovementSystem;
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

    void Start()
    {
        // Calculate the forward and backward directions with a slight angle
        forwardDirection = Quaternion.Euler(ANGLE_OFFSET, 5, 0) * Vector3.up;
        backwardDirection = Quaternion.Euler(-ANGLE_OFFSET, 5, 0) * Vector3.up;
        leftDirection = Quaternion.Euler(0, 5, -ANGLE_OFFSET) * Vector3.up;
        rightDirection = Quaternion.Euler(0, 5, ANGLE_OFFSET) * Vector3.up;

        _collisionLayer = LayerMask.GetMask("Collision");
        _chunkPositionPrevious = ChunkSystem._chunkPosition;

        _player = GameObject.Find("player");
        _playerMovementSystem = _player.GetComponent<PlayerMovementSystem>();
        _lightIndoor = GameObject.Find("light_indoor"); 
        _lightSelf = GameObject.Find("light_self");   
        
        _camera = GameObject.Find("main_camera"); 
        _volume = _camera.GetComponent<Volume>();
        HandleLight(false); 
    }

    void Update()
    {
        HandleInput();

        if (_playerMovementSystem._verticalVelocity == 0)
        {
            HandleObstructionCheck();
            HandleCheck(); 
        }
    }
 
    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Z)) _forceCull = !_forceCull;
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            _visionHeight += (scroll > 0) ? 1 : -1;
            if (_visionHeight > 3) _visionHeight = 3;
            if (_visionHeight < 0) _visionHeight = 0;
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
            else if (_yCheck && _yThresholdPrevious != _yThreshold) //change y threshold
            {  
                UpdateYCull();
            }
            else if (_yCheck && ChunkSystem._chunkPosition != _chunkPositionPrevious) //player moved
            { 
                UpdateYCull();
                _chunkPositionPrevious = ChunkSystem._chunkPosition;
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
        foreach (Transform child in transform)
        {   
            child.GetComponent<ChunkYCullHandler>().CullMeshAsync(); 
        }   
    }
   
    async void UpdateYCullDelayed(bool yCheckPrevious)
    { 
        if (_delayBuffer) return;
        _delayBuffer = true;

        await Task.Delay(80);
        if (_yCheck == yCheckPrevious)
        {
            _signalUpdateSpriteYCull?.Invoke();   
            _cullSyncFrame = Time.frameCount + CULL_SYNC_DELAY;  
            foreach (Transform child in transform)
            {   
                child.GetComponent<ChunkYCullHandler>().CullMeshAsync(); 
            }  
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
        playerPosition = _player.transform.position;
        if (_forceCull)
        {
            _yCheck = true;
            _yThreshold = (int)playerPosition.y + _visionHeight;
            return;
        }

        // Check each ray hit and return early if conditions are met
        rayForward = new Ray(playerPosition, forwardDirection);  
        if (!Physics.Raycast(rayForward, out _, 50, _collisionLayer)) {

            _yCheck = false;
            _yThreshold = 0;
            return;
            //fail
        }

        rayBackward = new Ray(playerPosition, backwardDirection); 
        if (!Physics.Raycast(rayBackward, out _, 50, _collisionLayer)) {
            
            rayToCamera = new Ray(playerPosition, _camera.transform.position - playerPosition);
            if (!Physics.Raycast(rayToCamera, out _, 50, _collisionLayer))
            {
                _yCheck = false;
                _yThreshold = 0;
                return;
                //fail
            }

            _yCheck = true;
            _yThreshold = (int)playerPosition.y + _visionHeight;
            return; // camera and forward
        } 

        rayLeft = new Ray(playerPosition, leftDirection); 
        if (!Physics.Raycast(rayLeft, out _, 50, _collisionLayer)) {

            _yCheck = false;
            _yThreshold = 0;
            return;
            //fail
        }

        rayRight = new Ray(playerPosition, rightDirection); 
        if (!Physics.Raycast(rayRight, out _, 50, _collisionLayer)) {

            _yCheck = false;
            _yThreshold = 0;
            return;
            //fail
        }

        _yCheck = true;
        _yThreshold = (int)playerPosition.y + _visionHeight;
        return; // all 4 directions no camera
    }


 


    public void ForceRevertMesh() // for when player fall into void
    {
        _yCheck = false;
        _yThreshold = 0;
        HandleLight(false); 
        UpdateYCull(); 
    }

}
