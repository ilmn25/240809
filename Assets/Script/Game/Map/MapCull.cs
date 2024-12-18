using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

public class MapCull
{ 
    private static readonly int CullSyncDelay = 3;  
    private static readonly float AngleOffset = 12; // Angle in degrees 
    private static bool _delayBuffer;
    public static event Action SignalUpdateSpriteYCull;
 
    public static int CullSyncFrame = 0;  
    public static bool YCheck = false;   
    public static int YThreshold = 0; 
    
    private static int _yThresholdPrevious = 0;  
    private static bool _yCheckPrevious = false;   
    private static Vector3 _chunkPositionPrevious;   
    private static int _visionHeight = 5;
    private enum CullMode { On, Off, Auto}
    private static CullMode _currentCullMode = CullMode.Auto;
    
    private static GameObject LightIndoor;
    private static GameObject LightSelf;
    private static Volume Volume;
    private static readonly Vector3 ForwardDirection = Quaternion.Euler(AngleOffset, 5, 0) * Vector3.up;
    private static readonly Vector3 BackwardDirection = Quaternion.Euler(-AngleOffset, 5, 0) * Vector3.up;
    private static readonly Vector3 LeftDirection = Quaternion.Euler(0, 5, -AngleOffset) * Vector3.up;
    private static readonly Vector3 RightDirection = Quaternion.Euler(0, 5, AngleOffset) * Vector3.up;
    
    private static Vector3 _playerPosition;
 
    public static void Initialize()
    {
        new CoroutineTask(YCheckRoutine());
        ViewPort.OnOrbitRotate += HandleObstructionCheck; 
 
        _chunkPositionPrevious = Scene.PlayerChunkPosition;

        LightIndoor = Game.Player.transform.Find("light_indoor").gameObject;
        LightSelf = Game.Player.transform.Find("light_self").gameObject;   
        Volume = Game.CameraObject.gameObject.GetComponent<Volume>();
        
        HandleLight(false); 
    }
    
    public static void Update() 
    {
        if (Control.Inst.CullMode.KeyDown()) {
            switch (_currentCullMode)
            {
                case CullMode.On:
                    _currentCullMode = CullMode.Off;
                    break;
                case CullMode.Off:
                    _currentCullMode = CullMode.Auto;
                    break;
                case CullMode.Auto:
                    _currentCullMode = CullMode.On;
                    break;
            }
        }
    }

    private static IEnumerator YCheckRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.05f);
            HandleObstructionCheck();
            HandleCheck();
        }
    }
      
    static void HandleCheck()
    {
        if (Time.frameCount > CullSyncFrame + 1)
        {
            if (YThreshold < (int)_playerPosition.y)
            {
                YThreshold = (int)_playerPosition.y;
                _visionHeight = YThreshold;
            }
            else if (YThreshold > (int)_playerPosition.y + 10)
            {
                YThreshold = (int)_playerPosition.y + 10;
                _visionHeight = YThreshold;
            }
            
            if (!_yCheckPrevious && YCheck) // enter
            {
                // HandleLight(true);
                UpdateYCullDelayed(YCheck);
            }
            else if (_yCheckPrevious && !YCheck) // exit
            {
                // HandleLight(false);
                UpdateYCullDelayed(YCheck);
            }
            else if (YCheck)   
            {
                if (Scene.PlayerChunkPosition != _chunkPositionPrevious)
                { 
                    UpdateYCull();
                    _chunkPositionPrevious = Scene.PlayerChunkPosition;
                }
                else if (_yThresholdPrevious != YThreshold)
                {
                    UpdateYCull();
                }
            }

            _yCheckPrevious = YCheck;
            _yThresholdPrevious = YThreshold;  
        }
    }

    static void UpdateYCull()
    {  
        SignalUpdateSpriteYCull?.Invoke();   
        CullSyncFrame = Time.frameCount + CullSyncDelay;
        foreach (var kvp in MapLoad.ActiveChunks) 
        { 
            kvp.Value.CullMeshAsync();
        }  
    }

    static async void UpdateYCullDelayed(bool yCheckPrevious)
    { 
        if (_delayBuffer) return;
        _delayBuffer = true;

        await Task.Delay(65);
        if (YCheck == yCheckPrevious)
        { 
            UpdateYCull();
        } 
        _delayBuffer = false;
    }

    static void HandleLight(bool flag)
    {
        LightIndoor.SetActive(flag);
        LightSelf.SetActive(flag); 
        Volume.profile = flag? Resources.Load<VolumeProfile>("shader/preset/volume_indoor") : Resources.Load<VolumeProfile>("shader/preset/volume_outdoor"); 
    }

    static void HandleObstructionCheck()
    {
        if (_currentCullMode == CullMode.Off)
        {
            YCheck = false;
            return;
        }
        
        _playerPosition = Game.Player.transform.position;
        if (_currentCullMode == CullMode.On)
        {
            YCheck = true;
            YThreshold = _visionHeight;
            return;
        }
        
        Vector3 cameraPosition = _playerPosition +  (Quaternion.Euler(0, ViewPort.OrbitRotation, 0) * new Vector3(0, ViewPort.Distance, -ViewPort.Distance));
        if (Physics.Raycast(new Ray(_playerPosition + Vector3.up * 0.9f,  cameraPosition - (_playerPosition + Vector3.up * 0.9f))
                , out _, Vector3.Distance(_playerPosition + Vector3.up * 0.9f, cameraPosition), Game.MaskMap))
        {
            // head then feet check
            if (Physics.Raycast(new Ray(_playerPosition,  cameraPosition - _playerPosition), out _, Vector3.Distance(_playerPosition, cameraPosition), Game.MaskMap))
            {
                YCheck = true;
                YThreshold = _visionHeight;
                return;
            }
        }
        
        if (Physics.Raycast(new Ray(_playerPosition, ForwardDirection), out _, 50, Game.MaskMap) &&
            Physics.Raycast(new Ray(_playerPosition, BackwardDirection), out _, 50, Game.MaskMap) &&
            Physics.Raycast(new Ray(_playerPosition, LeftDirection), out _, 50, Game.MaskMap) &&
            Physics.Raycast(new Ray(_playerPosition, RightDirection), out _, 50, Game.MaskMap)) {
            YCheck = true;
            YThreshold = _visionHeight;
            return;
        }
     
        YCheck = false; 
    }
            
    public static void ForceRevertMesh() // for when player fall into void
    {
        YCheck = false;
        YThreshold = 0;
        HandleLight(false); 
        UpdateYCull(); 
    }

    public static void HandleScrollInput(float scroll)
    {
        _visionHeight += (scroll > 0) ? 1 : -1;
    }
}
