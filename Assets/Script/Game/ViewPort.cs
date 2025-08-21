using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class ViewPort 
{
    public const float Distance = 26;
    private const int FOV = 55;
    private const float TiltDegreeX = 0.15f;
    private const float TiltDegreeY = 0.2f;
    private const float PanDegree = 1f;
    private const float TiltSpeed = 1f;
    private const float FollowSpeed = 10f;  
    private const int FOVChangeSpeed = 32; 
    
    public static int OrbitRotation = 0;
    public static Quaternion CurrentRotation;
    public static event Action UpdateOrbitRotate;
    public static event Action OnOrbitRotate;
    
    private static Coroutine _orbitCoroutine; 
    private static float _screenWidth;
    private static float _screenHeight; 
    private static float _targetFOV = 24;
    private static float _sinAngle = 0;
    private static float _cosAngle = 0;
    private static bool _isOrbiting = false;
    private static Quaternion _targetRotation;


    public static void Initialize()
    {  
        Game.Camera.transparencySortMode = TransparencySortMode.CustomAxis; 
        _screenWidth = Screen.width;
        _screenHeight = Screen.height;

        UpdateOrbit();
    }

    public static void Update()
    {   
        Game.Camera.fieldOfView = Mathf.Lerp(Game.Camera.fieldOfView, _targetFOV, Time.deltaTime * 5);
        
        HandlePlayerFollow(); 
        HandleCameraSway();

        if (Control.Inst.OrbitLeft.KeyDown())
        { 
            OrbitRotation += 45;
            if (OrbitRotation >= 180) OrbitRotation = -180;
            UpdateOrbit();
            OnOrbitRotate?.Invoke();
 
        }
        else if (Control.Inst.OrbitRight.KeyDown())
        {
            OrbitRotation -= 45;
            if (OrbitRotation <= -180) OrbitRotation = 180;
            UpdateOrbit();
            OnOrbitRotate?.Invoke();
        }
        else Game.ViewPortObject.transform.rotation = Quaternion.Lerp(Game.ViewPortObject.transform.rotation, Quaternion.Euler(0, OrbitRotation, 0), Time.deltaTime * 7);

        HandleOrbit();
    }
 
    public static void HandleScrollInput(float input)
    {  
        // DISTANCE -= input * FOV_CHANGE_SPEED;
        _targetFOV -= input * FOVChangeSpeed;
        _targetFOV = Mathf.Clamp(_targetFOV, 10, FOV);
    }
    
    public static Vector2 GetRelativeDirection(Vector2 direction)
    {
        // Normalize input to avoid scaling issues
        direction.Normalize();
 
        // Rotate the vector counter-clockwise by orbitRotation
        float rotatedX = direction.x * _cosAngle - direction.y * _sinAngle;
        float rotatedY = direction.x * _sinAngle + direction.y * _cosAngle;

        Vector2 rotated = new Vector2(rotatedX, rotatedY);

        // Optional: Snap to cardinal directions if needed
        rotated.x = Mathf.Abs(rotated.x) < 0.1f ? 0 : Mathf.Sign(rotated.x);
        rotated.y = Mathf.Abs(rotated.y) < 0.1f ? 0 : Mathf.Sign(rotated.y); 

        return rotated;
    }


 

    private static void UpdateOrbit()
    {
        HandleChangeSortAxis();

        float orbitRotation = Mathf.Deg2Rad * OrbitRotation;
        _cosAngle = Mathf.Cos(orbitRotation);
        _sinAngle = Mathf.Sin(orbitRotation);

        _targetRotation = Quaternion.Euler(0, ViewPort.OrbitRotation, 0);
        _isOrbiting = true;
    }

    private static void HandleOrbit()
    {
        if (_isOrbiting)
        {
            if (Quaternion.Angle(Game.ViewPortObject.transform.rotation, _targetRotation) > 0.1f)
            {
                CurrentRotation = Quaternion.Lerp(Game.ViewPortObject.transform.rotation, _targetRotation, Time.deltaTime * 7);
                UpdateOrbitRotate?.Invoke();
            }
            else
            {
                Game.ViewPortObject.transform.rotation = _targetRotation; // Ensure final rotation is exact
                _isOrbiting = false;
            }
        }
    }
 
    private static void HandleChangeSortAxis()
    {
        int zAxis = (OrbitRotation >= -90 && OrbitRotation <= 90) ? 1 : -1;
        int xAxis = (OrbitRotation > 0) ? 1 : -1;

        Game.Camera.transparencySortAxis = new Vector3(xAxis, 0, zAxis);
    }

    private static void HandleCameraSway()
    {
        float mouseX = Input.mousePosition.x;
        float mouseY = Input.mousePosition.y;
        
        float angleX = mouseX / _screenWidth * 2 - 1; // Normalize to range [-1, 1]
        float angleY = mouseY / _screenHeight * 2 - 1; // Normalize to range [-1, 1]

        float newRotationY =  angleX * TiltDegreeY;
        float newRotationX =  angleY * TiltDegreeX;

        Game.CameraObject.transform.localRotation = Quaternion.Lerp(Game.CameraObject.transform.localRotation, 
            Quaternion.Euler(45 - newRotationX, newRotationY , 0), Time.deltaTime * TiltSpeed);
        Game.CameraObject.transform.localPosition = Vector3.Lerp(Game.CameraObject.transform.localPosition, 
            new Vector3(newRotationY * PanDegree, Distance, -Distance), Time.deltaTime * TiltSpeed);
    }

    private static void HandlePlayerFollow()
    {
        Game.ViewPortObject.transform.position = Vector3.Lerp(Game.ViewPortObject.transform.position, Game.PlayerInfo.position, Time.deltaTime * FollowSpeed);
    }
 
}