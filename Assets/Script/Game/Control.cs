using System;
using UnityEngine;
 
[Serializable]
public class Control
{
    private const int InteractRange = 16;
    public static Control Inst = new Control(); 
    public static int CurrentPlayerIndex = 0;
    
    private static RaycastHit _mouseRaycastInfo;
    public static Vector3 MouseDirection; //direction of ray from camera to mouse target 
    public static Vector3 MousePosition; //position of mouse target 
    public static Transform MouseTarget;
    public static int MouseLayer; // -1 means hit void
    
    public readonly ControlKey Inv = new (KeyCode.Tab);
    public readonly ControlKey SwapChar = new (KeyCode.BackQuote);
    public readonly ControlKey Pause = new (KeyCode.Escape);
    public readonly ControlKey FullScreen = new (KeyCode.F11);
    public readonly ControlKey ActionPrimary = new (KeyCode.Mouse0);
    public readonly ControlKey ActionSecondary = new (KeyCode.Mouse1);
    public readonly ControlKey ActionPrimaryNear = new (KeyCode.G);
    public readonly ControlKey ActionSecondaryNear = new (KeyCode.F);
    public readonly ControlKey OrbitLeft = new (KeyCode.Q);
    public readonly ControlKey OrbitRight = new (KeyCode.E);
    public readonly ControlKey CullUp = new (KeyCode.Mouse4);
    public readonly ControlKey CullDown = new (KeyCode.Mouse3);
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
        Helper.FileSave(Inst, "KeyBinds");
    }

    public static void Load()
    {
        Inst = Helper.FileLoad<Control>("KeyBinds");
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
        Inst = Helper.FileLoad<Control>("KeyBinds") ?? new Control();
    }

    public static void SetPlayer(int i)
    {
        Game.PlayerInfo = World.Inst.target[i];
        Game.PlayerInfo.PathingStatus = PathingStatus.Stuck;
        Game.Player = null;
        GUIMain.StorageInv.Storage = Game.PlayerInfo.storage;
        GUIBar.Update();
        CurrentPlayerIndex = i;
    }
    
    public static void Update()
    {
        if (Inst.SwapChar.KeyDown())
        { 
            if (CurrentPlayerIndex == World.Inst.target.Count - 1)
                SetPlayer(0);
            else
                SetPlayer(CurrentPlayerIndex + 1);
        }
        
        if (Inst.FullScreen.KeyDown())
        {
            if (Screen.fullScreen)
                Screen.SetResolution(960, 540, false);
            else
                Screen.SetResolution(1920, 1080, true);
        }

        HandleActionButton();
        
        HandleScroll();
        
        HandleRaycast(); 
        
        HandleInput();
    }

    private static void HandleActionButton()
    {
        if (Inst.ActionPrimaryNear.KeyDown())
        { 
            IActionPrimaryResource target = GetNearestInteractable<IActionPrimaryResource>();
            if (target == null) return;
            Game.PlayerInfo.Target = ((EntityMachine)target).Info;  
            Game.PlayerInfo.ActionType = IActionType.Hit;
        }
        else if (Inst.ActionSecondaryNear.KeyDown() && !Dialogue.Showing)
        { 
            IActionSecondary target = GetNearestInteractable<IActionSecondaryPickUp>();
            if (target == null) return;
            Game.PlayerInfo.Target = ((EntityMachine)target).Info;   
            Game.PlayerInfo.ActionType = IActionType.PickUp;
        }
        // else if (Inst.ActionSecondaryNear.KeyDown() && !GUIDialogue.Showing)
        // { 
        //     IActionSecondary target = GetNearestInteractable<IActionSecondary>();
        //     if (target == null) return;
        //     Game.PlayerInfo.Target = ((EntityMachine)target).Info;   
        //     
        //     if (target is IActionSecondaryPickUp)
        //         Game.PlayerInfo.ActionType = IActionType.PickUp;
        //     else
        //         Game.PlayerInfo.ActionType = IActionType.Interact;
        // }
    }

    private static T GetNearestInteractable<T>() where T : class, IAction
    {
        Collider[] hitColliders = Physics.OverlapBox(Game.Player.transform.position, Vector3.one * InteractRange, Quaternion.identity, Game.MaskEntity);
        float distance, nearestDistance = InteractRange * InteractRange;
        T target, nearTarget = null;
        foreach (Collider collider in hitColliders)
        {
            if (collider.gameObject == Game.Player) continue;
            target = collider.gameObject.GetComponent<T>();
            if (target == null) continue;
            distance = Helper.SquaredDistance(collider.transform.position, Game.Player.transform.position);
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
        if (scroll == 0 || GUIMain.GUILoad.Showing) return;
        
        if (!Input.GetKey(KeyCode.LeftAlt))
        {
            // MapCull.HandleScrollInput(scroll);
            ViewPort.HandleScrollInput(scroll); 
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
        
        if (MouseTarget && Vector3.Distance(MousePosition, Game.ViewPortObject.transform.position) < InteractRange)
        {
            // if (Inst.ActionPrimary.KeyDown() && (Info.Action = MouseTarget.GetComponent<IHitBox>()) != null)
            // {
            //     Info.Target = MouseTarget;
            //     Info.ActionTarget = IActionTarget.Hit;
            // }
    
            if (Inst.ActionSecondary.KeyDown() && MouseTarget.gameObject != Game.Player && Game.PlayerInfo.Machine.IsCurrentState<DefaultState>())
            { 
                IAction action = MouseTarget.GetComponent<IActionSecondary>();
                if (action != null)
                {
                    Game.PlayerInfo.Target = ((EntityMachine)action).Info;
                    if (action is IActionSecondaryPickUp)
                        Game.PlayerInfo.ActionType = IActionType.PickUp;
                    else
                        Game.PlayerInfo.ActionType = IActionType.Interact;
                } 
            }
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
