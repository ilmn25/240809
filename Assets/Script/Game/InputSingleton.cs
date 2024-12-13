 
using System;
using UnityEngine;
 
public class InputSingleton : MonoBehaviour
{
    private Vector3 _thresholdPoint;
    private RaycastHit _targetInfo;
    
    private GameObject _gameObject;
    private Vector3 _direction;
    private Vector3 _position;
    private int _layerMask;
    
    private int RANGE = 5;
    
    private void Update()
    {
        HandleScroll();
        
        if (Game.GUIBusy) return;
        HandleRaycast(); 
        // if (Input.GetMouseButtonDown(0))
        //     Lib.Log(_layerMask, _gameObject, _position, IsInRange());
        if (_layerMask != -1 && IsInRange())
            HandleInput();
    }

    private void HandleScroll()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll == 0) return;

        if (Game.GUIBusy)
        {
            GUICraftingSingleton.Instance.HandleScrollInput(scroll);
        }
        else if (Input.GetMouseButton(1))
        { 
            CameraSingleton.Instance.HandleScrollInput(scroll); 
        }
        else if (!Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.LeftShift))
        {
            MapCullSingleton.Instance.HandleScrollInput(scroll);
        }  
        else
        {
            InventorySingleton.Instance.HandleScrollInput(scroll);
        }
    }
    
    private void HandleRaycast()
    { 
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        if (MapCullSingleton.Instance._yCheck)
        {
            // Calculate the position in the camera's direction where y = yThreshold 
            float yThreshold = MapCullSingleton.Instance._yThreshold + 0.05f;
            _thresholdPoint = ray.origin + ray.direction * ((yThreshold - ray.origin.y) / ray.direction.y);
            
            if (!WorldSingleton.Instance.GetBoolInMap(Vector3Int.FloorToInt(_thresholdPoint) + Vector3Int.down))
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
            // _layerMask = _targetInfo.collider.gameObject.layer;
            _layerMask = _targetInfo.collider.includeLayers;
            _gameObject = _targetInfo.collider.gameObject;
            _position = _targetInfo.point;
            _direction = ray.direction;
        }
        else
            _layerMask = -1;
    }
    
    bool IsInRange()
    { 
        if (Mathf.Abs(_position.x - Game.Player.transform.position.x) > RANGE ||
            Mathf.Abs(_position.y - Game.Player.transform.position.y) > RANGE ||
            Mathf.Abs(_position.z - Game.Player.transform.position.z) > RANGE)
        {
            return false;
        }
        return true;
    }

    private void HandleInput()
    {
        if (Game.isLayer(_layerMask, Game.IndexMap))
        {
            PlayerChunkEditSingleton.Instance.HandlePositionInfo(_position,  _direction);
            if (Input.GetMouseButtonDown(0))
            {
                PlayerChunkEditSingleton.Instance.HandleMapBreak(); 
            }
            else if (Input.GetMouseButtonDown(1))
            {
                PlayerChunkEditSingleton.Instance.HandleMapPlace();
            }
        }
        
        if (Input.GetMouseButtonDown(0))
            _targetInfo.collider.gameObject.GetComponent<LeftClickable>()?.OnLeftClick(); 
        if (Input.GetMouseButtonDown(1))
            _targetInfo.collider.gameObject.GetComponent<RightClickable>()?.OnRightClick(); 
    }
      
}
