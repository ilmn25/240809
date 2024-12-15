using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering; 

public class MapCullSingleton : MonoBehaviour
{ 
    public static MapCullSingleton Instance { get; private set; }  
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
    private int _visionHeight = 5;
    private enum CullMode { On, Off, Both}
    private CullMode _currentCullMode = CullMode.Both;
    
    private GameObject _lightIndoor;
    private GameObject _lightSelf;

    private GameObject _camera;
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
        StartCoroutine(yCheckRoutine());
        CameraSingleton.OnOrbitRotate += HandleObstructionCheck;
    }
    private IEnumerator yCheckRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.05f);
            HandleObstructionCheck();
            HandleCheck();
        }
    }
    void Start()
    {
        forwardDirection = Quaternion.Euler(ANGLE_OFFSET, 5, 0) * Vector3.up;
        backwardDirection = Quaternion.Euler(-ANGLE_OFFSET, 5, 0) * Vector3.up;
        leftDirection = Quaternion.Euler(0, 5, -ANGLE_OFFSET) * Vector3.up;
        rightDirection = Quaternion.Euler(0, 5, ANGLE_OFFSET) * Vector3.up;
 
        _chunkPositionPrevious = WorldSingleton._playerChunkPos;

        _lightIndoor = Game.Player.transform.Find("light_indoor").gameObject;
        _lightSelf = Game.Player.transform.Find("light_self").gameObject;   
        
        _camera = Game.Camera.gameObject; 
        _volume = _camera.GetComponent<Volume>();
        
        HandleLight(false); 
    }
 
    void Update() 
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
          
    }

    void HandleCheck()
    {
        if (Time.frameCount > _cullSyncFrame + 1)
        {
            if (_yThreshold < (int)playerPosition.y)
            {
                _yThreshold = (int)playerPosition.y;
                _visionHeight = _yThreshold;
            }
            else if (_yThreshold > (int)playerPosition.y + 10)
            {
                _yThreshold = (int)playerPosition.y + 10;
                _visionHeight = _yThreshold;
            }
            
            if (!_yCheckPrevious && _yCheck) // enter
            {
                // HandleLight(true);
                UpdateYCullDelayed(_yCheck);
            }
            else if (_yCheckPrevious && !_yCheck) // exit
            {
                // HandleLight(false);
                UpdateYCullDelayed(_yCheck);
            }
            else if (_yCheck)   
            {
                if (WorldSingleton._playerChunkPos != _chunkPositionPrevious)
                { 
                    UpdateYCull();
                    _chunkPositionPrevious = WorldSingleton._playerChunkPos;
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
        foreach (var kvp in MapLoadSingleton.Instance._activeChunks) 
        { 
            kvp.Value.CullMeshAsync();
        }  
    }  

    async void UpdateYCullDelayed(bool yCheckPrevious)
    { 
        if (_delayBuffer) return;
        _delayBuffer = true;

        await Task.Delay(65);
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
            _yThreshold = _visionHeight;
            return;
        }
        
        Vector3 cameraPosition = playerPosition +  (Quaternion.Euler(0, CameraSingleton._orbitRotation, 0) * new Vector3(0, CameraSingleton.DISTANCE, -CameraSingleton.DISTANCE));
        rayToCamera = new Ray(playerPosition + Vector3.up * 0.9f,  cameraPosition - (playerPosition + Vector3.up * 0.9f));
        if (Physics.Raycast(rayToCamera, out _, Vector3.Distance(playerPosition + Vector3.up * 0.9f, cameraPosition), Game.MaskMap))
        {
            // head then feet check
            rayToCamera = new Ray(playerPosition,  cameraPosition - playerPosition);
            if (Physics.Raycast(rayToCamera, out _, Vector3.Distance(playerPosition, cameraPosition), Game.MaskMap))
            {
                _yCheck = true;
                _yThreshold = _visionHeight;
                return;
            }
        }
        
        rayForward = new Ray(playerPosition, forwardDirection);
        rayBackward = new Ray(playerPosition, backwardDirection);
        rayLeft = new Ray(playerPosition, leftDirection);
        rayRight = new Ray(playerPosition, rightDirection);
        if (Physics.Raycast(rayForward, out _, 50, Game.MaskMap) &&
            Physics.Raycast(rayBackward, out _, 50, Game.MaskMap) &&
            Physics.Raycast(rayLeft, out _, 50, Game.MaskMap) &&
            Physics.Raycast(rayRight, out _, 50, Game.MaskMap)) {
            _yCheck = true;
            _yThreshold = _visionHeight;
            return;
        }
     
        _yCheck = false; 
    }
 

            
    public void ForceRevertMesh() // for when player fall into void
    {
        _yCheck = false;
        _yThreshold = 0;
        HandleLight(false); 
        UpdateYCull(); 
    }

    public void HandleScrollInput(float scroll)
    {
        _visionHeight += (scroll > 0) ? 1 : -1;
    }
}
