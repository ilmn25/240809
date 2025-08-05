using System;
using UnityEngine;
 
[Serializable]
public class Control
{
    private const int InteractRange = 3;
    public static Control Inst = new Control();
    
    private static RaycastHit _mouseRaycastInfo;
    public static Vector3 MouseDirection; //direction of ray from camera to mouse target 
    public static Vector3 MousePosition; //position of mouse target 
    public static Transform MouseTarget;
    public static int MouseLayer; // -1 means void
    
    public readonly ControlKey Inv = new (KeyCode.Tab);
    public readonly ControlKey Pause = new (KeyCode.Escape);
    public readonly ControlKey DigUp = new (KeyCode.Mouse4);
    public readonly ControlKey DigDown = new (KeyCode.Mouse3);
    public readonly ControlKey ActionPrimary = new (KeyCode.Mouse0);
    public readonly ControlKey ActionSecondary = new (KeyCode.Mouse1);
    public readonly ControlKey ActionPrimaryNear = new (KeyCode.F);
    public readonly ControlKey ActionSecondaryNear = new (KeyCode.G);
    public readonly ControlKey OrbitLeft = new (KeyCode.Q);
    public readonly ControlKey OrbitRight = new (KeyCode.E);
    public readonly ControlKey CullMode= new (KeyCode.Z);
    public readonly ControlKey Up = new (KeyCode.W);
    public readonly ControlKey Down = new (KeyCode.S);
    public readonly ControlKey Left = new (KeyCode.A);
    public readonly ControlKey Right = new (KeyCode.D);
    public readonly ControlKey Jump = new (KeyCode.Space); 
    public readonly ControlKey Sprint = new (KeyCode.LeftShift); 
    public readonly ControlKey Drop = new (KeyCode.R);
    public readonly ControlKey Hotbar = new (KeyCode.Tilde);
    public readonly ControlKey Hotbar1 = new (KeyCode.Alpha1);
    public readonly ControlKey Hotbar2 = new (KeyCode.Alpha2);
    public readonly ControlKey Hotbar3 = new (KeyCode.Alpha3);
    public readonly ControlKey Hotbar4 = new (KeyCode.Alpha4);
    public readonly ControlKey Hotbar5 = new (KeyCode.Alpha5);
    public readonly ControlKey Hotbar6 = new (KeyCode.Alpha6);
    public readonly ControlKey Hotbar7 = new (KeyCode.Alpha7);
    public readonly ControlKey Hotbar8 = new (KeyCode.Alpha8);
    public readonly ControlKey Hotbar9 = new (KeyCode.Alpha9);

    public static void Save()
    {
        Utility.Save(Inst, "KeyBinds");
    }

    public static void Load()
    {
        Inst = Utility.Load<Control>("KeyBinds");
    }
    
    public static Vector2Int GetMovementAxis()
    {
        Vector2Int movement = new Vector2Int();
        if (Inst.Up.Key()) movement += Vector2Int.up;
        if (Inst.Down.Key()) movement += Vector2Int.down;
        if (Inst.Left.Key()) movement += Vector2Int.left;
        if (Inst.Right.Key()) movement += Vector2Int.right;
        return movement;
    }

    public static void Initialize()
    {
        Inst = Utility.Load<Control>("KeyBinds") ?? new Control();
    }
    
    public static void Update()
    {
        if (Input.GetKeyDown(KeyCode.F11))
        {
            if (Screen.fullScreen)
                Screen.SetResolution(960, 540, false);
            else
                Screen.SetResolution(1920, 1080, true);
        }

        HandleActionButton();
        
        HandleScroll();
        
        if (GUI.Active) return;
        
        HandleRaycast(); 
        
        HandleInput();
    }

    private static void HandleActionButton()
    {
        if (Inst.ActionPrimaryNear.KeyDown())
        {
            GetNearestInteractable<IActionPrimary>()?.OnActionPrimary();  
        }
        else if (Inst.ActionSecondaryNear.KeyDown())
        { 
            GetNearestInteractable<IActionSecondary>()?.OnActionSecondary(); 
        }
    }

    private static T GetNearestInteractable<T>() where T : class, IAction
    {
        Collider[] hitColliders = Physics.OverlapBox(Game.Player.transform.position, Vector3.one * InteractRange, Quaternion.identity, Game.MaskEntity);
        float distance, nearestDistance = InteractRange * InteractRange;
        T target, nearTarget = null;
        foreach (Collider collider in hitColliders)
        {
            target = collider.gameObject.GetComponent<T>();
            if (target == null) continue;
            // Debug.Log(collider.gameObject.name);
            distance = Utility.SquaredDistance(collider.transform.position, Game.Player.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearTarget = target;
            }
        }
        return nearTarget; 
    }
 
    private static void HandleScroll()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel"); 
        if (scroll == 0) return;

        if (GUI.Active)
        {
            GUICraft.HandleScrollInput(scroll);
        }
        else if (Input.GetKey(KeyCode.Mouse1))
        { 
            ViewPort.HandleScrollInput(scroll); 
        }
        else if (!Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.LeftShift))
        {
            MapCull.HandleScrollInput(scroll);
        }  
        else
        {
            Inventory.HandleScrollInput(scroll);
        }
    }
    
    private static void HandleInput()
    {
        if (MouseLayer != -1 && MouseLayer != Game.MaskMap)
        {
            MouseTarget = _mouseRaycastInfo.collider.transform;
        }
        else MouseTarget = null;
        
        if (MouseTarget && Vector3.Distance(MousePosition, Game.Player.transform.position) < InteractRange)
        {
            if (Inst.ActionPrimary.KeyDown())
                MouseTarget.GetComponent<IActionPrimary>()?.OnActionPrimary(); 
            if (Inst.ActionSecondary.KeyDown())
                MouseTarget.GetComponent<IActionSecondary>()?.OnActionSecondary(); 
        } 
    }
    private static void HandleRaycast()
    { 
        Ray ray = Game.Camera.ScreenPointToRay(Input.mousePosition);
        
        if (MapCull.YCheck)
        {
            // Calculate the position in the camera's direction where y = yThreshold 
            float yThreshold = MapCull.YThreshold + 0.05f;
            Vector3 thresholdPoint = ray.origin + ray.direction * ((yThreshold - ray.origin.y) / ray.direction.y);
            
            if (!NavMap.Get(Vector3Int.FloorToInt(thresholdPoint) + Vector3Int.down))
            { 
                MouseLayer = Game.MaskMap;
                MousePosition = Vector3Int.FloorToInt(thresholdPoint); ;
                MouseDirection = Vector3.down;
                return;
            }
            ray = new Ray(thresholdPoint, ray.direction);
            Physics.Raycast(ray, out _mouseRaycastInfo);
        }
        else
        {
            Physics.Raycast(ray, out _mouseRaycastInfo);
        }

        if (_mouseRaycastInfo.collider)
        {
            MouseLayer = _mouseRaycastInfo.collider.includeLayers; 
            MousePosition = _mouseRaycastInfo.point;
            MouseDirection = ray.direction;
        }
        else
            MouseLayer = -1;
    }
 
 
      
}
