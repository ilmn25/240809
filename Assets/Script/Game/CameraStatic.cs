using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class CameraStatic : MonoBehaviour
{
    private GameObject _player; 
    public static int _orbitRotation = 0;
    public static Quaternion _currentRotation;
    public static event Action OnOrbitRotate;
    private Coroutine _orbitCoroutine; 
    
    private float _screenWidth;
    private float _screenHeight; 
    private Quaternion _targetRotation;
    private Vector3 _targetPosition;
    private GameObject _camera; // Use the new keyword to hide the inherited member 

    [SerializeField] private float TILT_DEGREE_X = 0.15f;
    [SerializeField] private float TILT_DEGREE_Y = 0.2f; // Maximum rotation angle
    [SerializeField] private float PAN_DEGREE = 1f; // Maximum rotation angle
    [SerializeField] private float TILT_SPEED = 1f; // Speed of rotation
    [SerializeField] private float FOLLOW_SPEED = 10f; // Speed of following the player
    [SerializeField] private float FOV_CHANGE_SPEED = 11f; // Speed of FOV change
 
    private static float _sinAngle = 0;
    private static float _cosAngle = 0; 


    void Start()
    {  
        _player = GameObject.Find("player"); 
        _screenWidth = Screen.width;
        _screenHeight = Screen.height;
        _camera = transform.Find("main_camera").gameObject;
        _targetRotation = _camera.transform.rotation;
        _targetPosition = _camera.transform.localPosition;
        _camera.GetComponent<Camera>().transparencySortMode = TransparencySortMode.CustomAxis; 

        UpdateOrbit();
    }

    void Update()
    {  
        HandlePlayerFollow(); 
        HandleCameraSway();
        HandleFOVChange();

        if (Input.GetKeyDown(KeyCode.Q))
        { 
            _orbitRotation += 45;
            if (_orbitRotation >= 180) _orbitRotation = -180;
            UpdateOrbit();
 
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            _orbitRotation -= 45;
            if (_orbitRotation <= -180) _orbitRotation = 180;
            UpdateOrbit();
 
        }
        else transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, _orbitRotation, 0), Time.deltaTime * 7);

        HandleOrbit();
    }
 

    private bool isOrbiting = false;
    private Quaternion targetRotation;
    void UpdateOrbit()
    {
        HandleChangeSortAxis();

        float orbitRotation = Mathf.Deg2Rad * _orbitRotation;
        _cosAngle = Mathf.Cos(orbitRotation);
        _sinAngle = Mathf.Sin(orbitRotation);

        targetRotation = Quaternion.Euler(0, CameraStatic._orbitRotation, 0);
        isOrbiting = true;
    }

    void HandleOrbit()
    {
        if (isOrbiting)
        {
            if (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
            {
                _currentRotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 7);
                OnOrbitRotate?.Invoke();
            }
            else
            {
                transform.rotation = targetRotation; // Ensure final rotation is exact
                isOrbiting = false;
            }
        }
    }


    void HandleFOVChange()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (!Input.GetKey(KeyCode.LeftShift) && scrollInput != 0)
        {
            Camera cameraComponent = _camera.GetComponent<Camera>();
            cameraComponent.fieldOfView -= scrollInput * FOV_CHANGE_SPEED;
            cameraComponent.fieldOfView = Mathf.Clamp(cameraComponent.fieldOfView, 6f, 20f); // Clamp FOV to reasonable values
        }
    }



  



    void HandleChangeSortAxis()
    {
        int zAxis = (_orbitRotation >= -90 && _orbitRotation <= 90) ? 1 : -1;
        int xAxis = (_orbitRotation > 0) ? 1 : -1;

        // CustomLibrary.Log(_orbitRotation, zAxis, xAxis);
        _camera.GetComponent<Camera>().transparencySortAxis = new Vector3(xAxis, 0, zAxis);
    }

    void HandleCameraSway()
    {
        float mouseX = Input.mousePosition.x;
        float mouseY = Input.mousePosition.y;
        
        float angleX = mouseX / _screenWidth * 2 - 1; // Normalize to range [-1, 1]
        float angleY = mouseY / _screenHeight * 2 - 1; // Normalize to range [-1, 1]

        float newRotationY =  angleX * TILT_DEGREE_Y;
        float newRotationX =  angleY * TILT_DEGREE_X;
        // UnityEngine.Debug.Log(_camera.transform.rotation.y); 
        _targetRotation = Quaternion.Euler(45 - newRotationX, newRotationY , 0);
        _targetPosition = new Vector3(newRotationY * PAN_DEGREE, 30, -30); 

        _camera.transform.localRotation = Quaternion.Lerp(_camera.transform.localRotation, _targetRotation, Time.deltaTime * TILT_SPEED);
        _camera.transform.localPosition = Vector3.Lerp(_camera.transform.localPosition, _targetPosition, Time.deltaTime * TILT_SPEED);
    } 

    void HandlePlayerFollow()
    {
        if (_player != null)
        {
            Vector3 targetPosition = _player.transform.position;
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * FOLLOW_SPEED);
        }
    }



    
    public static Vector2 GetRelativeDirection(Vector2 direction) 
    {
        Vector2 rotated = new Vector3();
        rotated.x = direction.x * _cosAngle - direction.y * _sinAngle;
        rotated.y = direction.x * _sinAngle + direction.y * _cosAngle; 
        return rotated;
    }

}