 
using System;
using UnityEngine;
 
public class InputHandler
{
    private static Vector3 _thresholdPoint;
    private static RaycastHit _targetInfo;
    
    private static GameObject _gameObject;
    private static Vector3 _direction;
    private static Vector3 _position;
    private static int _layerMask;

    private const int Range = 5;

    public static void Update()
    {
        if (Input.GetKeyDown(KeyCode.F11))
        {
            if (Screen.fullScreen)
            {
                Screen.SetResolution(960, 540, false);
            }
            else
            {
                Screen.SetResolution(1920, 1080, true);
            }
        }
        
        HandleScroll();
        
        if (GUI.GUIBusy) return;
        
        HandleRaycast(); 
        
        if (_layerMask != -1 && Scene.InPlayerBlockRange(_position, Range))
            HandleInput();
    }

    private static void HandleScroll()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel"); 
        if (scroll == 0) return;

        if (GUI.GUIBusy)
        {
            GUICraft.HandleScrollInput(scroll);
        }
        else if (Input.GetMouseButton(1))
        { 
            CameraSingleton.Instance.HandleScrollInput(scroll); 
        }
        else if (!Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.LeftShift))
        {
            MapCull.HandleScrollInput(scroll);
        }  
        else
        {
            InventorySingleton.HandleScrollInput(scroll);
        }
    }
    
    private static void HandleRaycast()
    { 
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        if (MapCull.YCheck)
        {
            // Calculate the position in the camera's direction where y = yThreshold 
            float yThreshold = MapCull.YThreshold + 0.05f;
            _thresholdPoint = ray.origin + ray.direction * ((yThreshold - ray.origin.y) / ray.direction.y);
            
            if (!NavMap.Get(Vector3Int.FloorToInt(_thresholdPoint) + Vector3Int.down))
            { 
                _layerMask = Game.MaskMap;
                _position = Vector3Int.FloorToInt(_thresholdPoint); ;
                _direction = Vector3.down;
                return;
            }
            ray = new Ray(_thresholdPoint, ray.direction);
            Physics.Raycast(ray, out _targetInfo);
        }
        else
        {
            Physics.Raycast(ray, out _targetInfo);
        }

        if (_targetInfo.collider)
        {
            _layerMask = _targetInfo.collider.includeLayers;
            _gameObject = _targetInfo.collider.gameObject;
            _position = _targetInfo.point;
            _direction = ray.direction;
        }
        else
            _layerMask = -1;
    }
 

    private static void HandleInput()
    {
        if (Utility.isLayer(_layerMask, Game.IndexMap))
        {
            PlayerTerraform.HandlePositionInfo(_position,  _direction);
            if (Input.GetMouseButtonDown(0))
            {
                PlayerTerraform.HandleMapBreak(); 
            }
            else if (Input.GetMouseButtonDown(1))
            {
                PlayerTerraform.HandleMapPlace();
            }
        }
        
        if (Input.GetMouseButtonDown(0))
            _targetInfo.collider.gameObject.GetComponent<ILeftClick>()?.OnLeftClick(); 
        if (Input.GetMouseButtonDown(1))
            _targetInfo.collider.gameObject.GetComponent<IRightClick>()?.OnRightClick(); 
    }
      
}
