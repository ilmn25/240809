using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class CameraSingleton : MonoBehaviour
{
    public static CameraSingleton Instance { get; private set; }  
    
    public static int _orbitRotation = 0;
    public static Quaternion _currentRotation;
    public static event Action UpdateOrbitRotate;
    public static event Action OnOrbitRotate;
    private Coroutine _orbitCoroutine; 
    private Camera _camera;
    
    private float _screenWidth;
    private float _screenHeight; 
    private Quaternion _targetRotation;
    private Vector3 _targetPosition;
    private float _targetFOV = 40;
    
    
    public static float DISTANCE = 26;
    private int FOV = 55;
    private float TILT_DEGREE_X = 0.15f;
    private float TILT_DEGREE_Y = 0.2f; // Maximum rotation angle
    private float PAN_DEGREE = 1f; // Maximum rotation angle
    private float TILT_SPEED = 1f; // Speed of rotation
    private float FOLLOW_SPEED = 10f; // Speed of following the player
    private int FOV_CHANGE_SPEED = 32; // Speed of FOV change
 
    private static float _sinAngle = 0;
    private static float _cosAngle = 0; 


    void Start()
    {  
        Instance = this;

        _camera = Game.Camera.GetComponent<Camera>();
        _camera.transparencySortMode = TransparencySortMode.CustomAxis; 
        _screenWidth = Screen.width;
        _screenHeight = Screen.height;
        _targetRotation = Game.Camera.transform.rotation;
        _targetPosition = Game.Camera.transform.localPosition; 

        UpdateOrbit();
    }

    void Update()
    {   
        
        _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, _targetFOV, Time.deltaTime * 5);
        
        HandlePlayerFollow(); 
        HandleCameraSway();

        if (Input.GetKeyDown(KeyCode.Q))
        { 
            _orbitRotation += 45;
            if (_orbitRotation >= 180) _orbitRotation = -180;
            UpdateOrbit();
            OnOrbitRotate?.Invoke();
 
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            _orbitRotation -= 45;
            if (_orbitRotation <= -180) _orbitRotation = 180;
            UpdateOrbit();
            OnOrbitRotate?.Invoke();
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

        targetRotation = Quaternion.Euler(0, CameraSingleton._orbitRotation, 0);
        isOrbiting = true;
    }

    void HandleOrbit()
    {
        if (isOrbiting)
        {
            if (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
            {
                _currentRotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 7);
                UpdateOrbitRotate?.Invoke();
            }
            else
            {
                transform.rotation = targetRotation; // Ensure final rotation is exact
                isOrbiting = false;
            }
        }
    }

    public void HandleScrollInput(float input)
    {  
        // DISTANCE -= input * FOV_CHANGE_SPEED;
        _targetFOV -= input * FOV_CHANGE_SPEED;
        _targetFOV = Mathf.Clamp(_targetFOV, 10, FOV);
    }

    void HandleChangeSortAxis()
    {
        int zAxis = (_orbitRotation >= -90 && _orbitRotation <= 90) ? 1 : -1;
        int xAxis = (_orbitRotation > 0) ? 1 : -1;

        // CustomLibrary.Log(_orbitRotation, zAxis, xAxis);
        Game.Camera.GetComponent<Camera>().transparencySortAxis = new Vector3(xAxis, 0, zAxis);
    }

    void HandleCameraSway()
    {
        float mouseX = Input.mousePosition.x;
        float mouseY = Input.mousePosition.y;
        
        float angleX = mouseX / _screenWidth * 2 - 1; // Normalize to range [-1, 1]
        float angleY = mouseY / _screenHeight * 2 - 1; // Normalize to range [-1, 1]

        float newRotationY =  angleX * TILT_DEGREE_Y;
        float newRotationX =  angleY * TILT_DEGREE_X;
        // UnityEngine.Debug.Log(Game.Camera.transform.rotation.y); 
        _targetRotation = Quaternion.Euler(45 - newRotationX, newRotationY , 0); 
        _targetPosition = new Vector3(newRotationY * PAN_DEGREE, DISTANCE, -DISTANCE); 

        Game.Camera.transform.localRotation = Quaternion.Lerp(Game.Camera.transform.localRotation, _targetRotation, Time.deltaTime * TILT_SPEED);
        Game.Camera.transform.localPosition = Vector3.Lerp(Game.Camera.transform.localPosition, _targetPosition, Time.deltaTime * TILT_SPEED);
    } 

    void HandlePlayerFollow()
    {
        if (Game.Player != null)
        {
            Vector3 targetPosition = Game.Player.transform.position;
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