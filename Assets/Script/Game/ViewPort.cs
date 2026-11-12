using System;
using UnityEngine;

public partial class ViewPort 
{
    public const float Distance = 26;

    /// <summary>Aspect ratio the world is always framed at. Any window with a different
    /// shape gets black bars instead of extra/less visible world.</summary>
    public const float TargetAspect = 16f / 9f;

    private const float TiltDegreeX = 0.15f;
    private const float TiltDegreeY = 0.2f;
    private const float PanDegree = 1f;
    private const float TiltSpeed = 1f;
    private const float FollowSpeed = 10f;  
    private const int FOVChangeSpeed = 32; 
    
    public static int OrbitRotation = 0;
    public static Quaternion CurrentRotation;

    /// <summary>Player the camera follows instead of the controlled one — set while the
    /// player list is hovered, so the view previews that party member.</summary>
    public static PlayerInfo FollowOverride;

    public static void SetFollowOverride(PlayerInfo player) => FollowOverride = player;

    public static event Action UpdateOrbitRotate;
    public static event Action OnOrbitRotate;
    
    private static Rect _viewRect = new Rect(0f, 0f, 1f, 1f);
    private static Texture2D _barTexture;
    private static float _targetFOV = 24;
    private static float _sinAngle = 0;
    private static float _cosAngle = 0;
    private static bool _isOrbiting = false;
    private static Quaternion _targetRotation;


    public static void Initialize()
    {  
        Main.Camera.transparencySortMode = TransparencySortMode.CustomAxis; 
        UpdateViewRect();

        UpdateOrbit();
    }

    public static void Update()
    {   
        UpdateViewRect();

        Main.Camera.fieldOfView = Mathf.Lerp(Main.Camera.fieldOfView, _targetFOV, Time.deltaTime * 5);

        ScreenShake.Update();
        
        HandlePlayerFollow(); 
        HandleCameraSway();

        if (Control.Inst.OrbitLeft.KeyDown())
        { 
            Tutorial.OnControl(Tutorial.TutorialControl.Orbit);
            OrbitRotation += 45;
            if (OrbitRotation >= 180) OrbitRotation = -180;
            UpdateOrbit();
            OnOrbitRotate?.Invoke();
 
        }
        else if (Control.Inst.OrbitRight.KeyDown())
        {
            Tutorial.OnControl(Tutorial.TutorialControl.Orbit);
            OrbitRotation -= 45;
            if (OrbitRotation <= -180) OrbitRotation = 180;
            UpdateOrbit();
            OnOrbitRotate?.Invoke();
        }
        else Main.ViewPortObject.transform.rotation = Quaternion.Lerp(Main.ViewPortObject.transform.rotation, Quaternion.Euler(0, OrbitRotation, 0), Time.deltaTime * 7);

        HandleOrbit();
    }
 
    public static void HandleScrollInput(float input)
    {  
        _targetFOV -= input * FOVChangeSpeed;
        _targetFOV = Mathf.Clamp(_targetFOV, 10, Settings.MaxFOVs[Settings.Inst.MaxFOVIndex]);
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
            if (Quaternion.Angle(Main.ViewPortObject.transform.rotation, _targetRotation) > 0.1f)
            {
                CurrentRotation = Quaternion.Lerp(Main.ViewPortObject.transform.rotation, _targetRotation, Time.deltaTime * 7);
                UpdateOrbitRotate?.Invoke();
            }
            else
            {
                Main.ViewPortObject.transform.rotation = _targetRotation; // Ensure final rotation is exact
                _isOrbiting = false;
            }
        }
    }
 
    private static void HandleChangeSortAxis()
    {
        int zAxis = (OrbitRotation >= -90 && OrbitRotation <= 90) ? 1 : -1;
        int xAxis = (OrbitRotation > 0) ? 1 : -1;

        Main.Camera.transparencySortAxis = new Vector3(xAxis, 0, zAxis);
    }

    private static void HandleCameraSway()
    {
        Rect view = Main.Camera.pixelRect;
        Vector3 mouse = Input.mousePosition;

        float angleX = Mathf.Clamp((mouse.x - view.xMin) / view.width * 2f - 1f, -1f, 1f);
        float angleY = Mathf.Clamp((mouse.y - view.yMin) / view.height * 2f - 1f, -1f, 1f);

        float newRotationY =  angleX * TiltDegreeY;
        float newRotationX =  angleY * TiltDegreeX;

        Main.CameraObject.transform.localRotation = Quaternion.Lerp(Main.CameraObject.transform.localRotation, 
            Quaternion.Euler(45 - newRotationX, newRotationY , 0), Time.deltaTime * TiltSpeed);
        Main.CameraObject.transform.localPosition = Vector3.Lerp(Main.CameraObject.transform.localPosition, 
            new Vector3(newRotationY * PanDegree, Distance, -Distance), Time.deltaTime * TiltSpeed);
    }

    private static void HandlePlayerFollow()
    {
        PlayerInfo target = Main.PlayerInfo;
        // Hovering a party slot previews that player; fall back to whoever we control as
        // soon as the hover ends or that player is gone/unspawned.
        if (FollowOverride != null && !FollowOverride.Destroyed && FollowOverride.Machine != null)
            target = FollowOverride;

        Main.ViewPortObject.transform.position = Vector3.Lerp(Main.ViewPortObject.transform.position, 
            target.position, Time.deltaTime * FollowSpeed) + ScreenShake.Offset;
    }

    /// <summary>Letterbox the cameras to <see cref="TargetAspect"/>, so the rendered
    /// world keeps the same framing whatever shape the window is.</summary>
    private static void UpdateViewRect()
    {
        if (Main.Camera == null) return;

        float windowAspect = (float)Screen.width / Mathf.Max(1, Screen.height);
        float scaleHeight = windowAspect / TargetAspect;

        Rect rect;
        if (scaleHeight < 1f)
        {
            // Window is taller than the target: bars on top and bottom.
            rect = new Rect(0f, (1f - scaleHeight) * 0.5f, 1f, scaleHeight);
        }
        else
        {
            // Window is wider than the target: bars on the left and right.
            float scaleWidth = 1f / scaleHeight;
            rect = new Rect((1f - scaleWidth) * 0.5f, 0f, scaleWidth, 1f);
        }

        if (rect == _viewRect) return;
        _viewRect = rect;

        Main.Camera.rect = rect;
        // The HUD camera is a URP overlay stacked on the world camera, so it inherits the
        // same viewport; assigning it keeps both in sync if the stack ever changes.
        if (Main.GUICamera != null) Main.GUICamera.rect = rect;
    }

    /// <summary>Call from Main.OnGUI() to paint the black bars outside the fixed-ratio view.</summary>
    public static void OnGUI()
    {
        UpdateViewRect();

        Rect view = Main.Camera.pixelRect;
        if (view.width <= 0f || view.height <= 0f) return;
        if (view.width >= Screen.width - 0.5f && view.height >= Screen.height - 0.5f) return;

        if (_barTexture == null)
        {
            _barTexture = new Texture2D(1, 1);
            _barTexture.SetPixel(0, 0, Color.black);
            _barTexture.Apply();
        }

        // Round outwards so no unpainted (stale) border is left by viewport rounding.
        float left = Mathf.Floor(view.xMin);
        float right = Screen.width - Mathf.Ceil(view.xMax);
        float bottom = Mathf.Floor(view.yMin);
        float top = Screen.height - Mathf.Ceil(view.yMax);

        Color prevColor = UnityEngine.GUI.color;
        UnityEngine.GUI.color = Color.black;

        // IMGUI's Y axis points down while Camera.pixelRect's points up, so the bottom
        // bar is drawn from the bottom edge of the screen.
        if (left > 0f) UnityEngine.GUI.DrawTexture(new Rect(0f, 0f, left, Screen.height), _barTexture);
        if (right > 0f) UnityEngine.GUI.DrawTexture(new Rect(Screen.width - right, 0f, right, Screen.height), _barTexture);
        if (bottom > 0f) UnityEngine.GUI.DrawTexture(new Rect(0f, Screen.height - bottom, Screen.width, bottom), _barTexture);
        if (top > 0f) UnityEngine.GUI.DrawTexture(new Rect(0f, 0f, Screen.width, top), _barTexture);

        UnityEngine.GUI.color = prevColor;
    }
 
}