using System;
using Mirror;
using UnityEngine;
 
[Serializable]
public class Control
{
    private const int InteractRange = 2;
    public static Control Inst = new Control(); 
    public static int CurrentPlayerIndex = 0;
    
    private static RaycastHit _mouseRaycastInfo;
    public static Vector3 MouseDirection; //direction of ray from camera to mouse target 
    public static Vector3 MousePosition; //position of mouse target 
    public static Transform MouseTarget;
    public static int MouseLayer; // -1 means hit void
    
    public readonly ControlKey Inv = new (KeyCode.F3);
    public readonly ControlKey Map = new (KeyCode.M);
    public readonly ControlKey SwapChar = new (KeyCode.Tab);
    public readonly ControlKey Recall = new (KeyCode.H);
    public readonly ControlKey Pause = new (KeyCode.Escape);
    public readonly ControlKey FullScreen = new (KeyCode.F11);
    public readonly ControlKey RevealMap = new (KeyCode.F7);
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
        Main.PlayerInfo = global::Save.Inst.players[i];
        Main.PlayerInfo.PathingStatus = PathingStatus.Stuck;
        GUIMain.StorageInv.Storage = Main.PlayerInfo.Storage;
        Inventory.RefreshInventory();
        GUIBar.Update();
        CurrentPlayerIndex = i;
    }

    /// <summary>Switch control to the player at index <paramref name="i"/> and
    /// update network ownership (host claims/releases, client re-claims).</summary>
    public static void SwitchToPlayer(int i)
    {
        if (i < 0 || i >= global::Save.Inst.players.Count) return;
        if (i == CurrentPlayerIndex) return;

        int prevIndex = CurrentPlayerIndex;
        // Drop the outgoing character's stale input direction so it doesn't keep
        // walking (e.g. when it was mid-jump with a key held on swap).
        PlayerInfo prev = global::Save.Inst.players[prevIndex];
        if (prev != null)
        {
            prev.Direction = Vector3.zero;
            prev.Velocity = new Vector3(0f, prev.Velocity.y, 0f);
        }
        SetPlayer(i);

        if (!Helper.IsHost())
        {
            // Immediately claim the new player so the host doesn't broadcast stale controllerId.
            PlayerSync.SendClientPlayerBatch();
        }
        else
        {
            // Host: update player ownership so clients see the change
            string prevUid = global::Save.Inst.players[prevIndex].uid;
            string newUid = global::Save.Inst.players[i].uid;
            PlayerSync.HostReleasePlayer(prevUid);
            PlayerSync.HostClaimPlayer(newUid);
        }
    }
    
    public static void Update()
    {
        if (Inst.SwapChar.KeyDown())
        { 
            Audio.PlaySFX(SfxID.Text);
            int prevIndex = CurrentPlayerIndex;
            int next = CurrentPlayerIndex;
            int count = global::Save.Inst.players.Count;
            for (int i = 0; i < count; i++)
            {
                next = (next + 1) % count;
                if (next == prevIndex) break; // wrapped around
                var p = global::Save.Inst.players[next];
                if (p.Machine != null) break;
            }
            if (next == prevIndex) return; // no other player in range
            Tutorial.OnSwap();
            SwitchToPlayer(next);
        }
        
        if (Inst.FullScreen.KeyDown())
        {
            if (Screen.fullScreen)
                Screen.SetResolution(960, 540, false);
            else
                Screen.SetResolution(1920, 1080, true);
        }

        if (Inst.RevealMap.KeyDown() && World.Inst?.Map != null)
        {
            Audio.PlaySFX(SfxID.Text);
            World.Inst.Map.ToggleFullReveal();
        }

        if (Inst.Recall.KeyDown())
        {
            Audio.PlaySFX(SfxID.Text);
            Tutorial.OnRecall();
            PlayerMachine.RecallAllies();
        }

        // The map handles its own input (drag/zoom) — no world interaction while open.
        if (GUIMain.Map is { IsOpen: true }) return;

        HandleActionButton();
        
        HandleScroll();
        
        HandleRaycast(); 
        
        HandleInput();

        if (Inst.ActionPrimary.KeyDown() && !GUIMain.IsHover &&
            Main.PlayerInfo?.Equipment?.Info.Type == ItemType.Tool &&
            Main.PlayerInfo.Machine is EntityMachine em &&
            (Helper.IsHost() || PlayerSync.CanLocalClientControl(Main.PlayerInfo.uid)))
        {
            em.Attack();
        }
    }

    private static void HandleActionButton()
    {
        // Spectating clients cannot interact
        if (!Helper.IsHost() && NetworkClient.isConnected &&
            Main.PlayerInfo != null &&
            !PlayerSync.CanLocalClientControl(Main.PlayerInfo.uid))
            return;

        if (Inst.ActionPrimaryNear.KeyDown())
        { 
            IActionPrimaryResource target = GetNearestInteractable<IActionPrimaryResource>();
            if (target == null) return;
            Main.PlayerInfo.Target = ((EntityMachine)target).Info;  
            Main.PlayerInfo.ActionType = IActionType.Hit;
        }
        else if (Inst.ActionSecondaryNear.KeyDown() && !Dialogue.Showing)
        {
            IActionSecondary target = GetNearestInteractable<IActionSecondaryPickUp>();
            if (target == null) return;
            var info = ((EntityMachine)target).Info;

            if (Helper.IsHost() || NetworkClient.isConnected)
            {
                // Host/Client: state machine handles pathfinding + pickup via MobChaseAction
                Main.PlayerInfo.Target = info;
                Main.PlayerInfo.ActionType = IActionType.PickUp;
            }
        }
    }

    private static T GetNearestInteractable<T>() where T : class, IAction
    {
        Collider[] hitColliders = Physics.OverlapBox(Main.Player.transform.position, Vector3.one * InteractRange, Quaternion.identity, Main.MaskEntity);
        float distance, nearestDistance = InteractRange * InteractRange;
        T target, nearTarget = null;
        foreach (Collider collider in hitColliders)
        {
            if (collider.gameObject == Main.Player) continue;
            target = collider.gameObject.GetComponent<T>();
            if (target == null) continue;
            // Skip non-pickupable items (blood pools) so they don't block picking
            // up a real item that's slightly farther away.
            if (target is EntityMachine em && em.Info is ItemInfo itemInfo && !itemInfo.item.Info.Pickupable)
                continue;
            distance = Helper.SquaredDistance(collider.transform.position, Main.Player.transform.position);
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
        if (MouseLayer != -1 && MouseLayer != Main.MaskMap)
        {
            MouseTarget = _mouseRaycastInfo.collider.transform;
        }
        else MouseTarget = null;
        
        if (MouseTarget && Vector3.Distance(MousePosition, Main.ViewPortObject.transform.position) < InteractRange)
        { 
    
            if (Inst.ActionSecondary.KeyDown() && !Dialogue.Showing && MouseTarget.gameObject != Main.Player && Main.PlayerInfo.Machine != null && Main.PlayerInfo.Machine.IsCurrentState<DefaultState>())
            { 
                IAction action = MouseTarget.GetComponent<IActionSecondary>();
                if (action != null)
                {
                    if (action is IActionSecondaryPickUp && ((EntityMachine)action).Info is ItemInfo itemInfo)
                    {
                        // Skip non-pickupable items (e.g. blood pools) — they can't be collected.
                        if (!itemInfo.item.Info.Pickupable) return;

                        // Both host and client: state machine handles pathfinding + pickup
                        Main.PlayerInfo.Target = ((EntityMachine)action).Info;
                        Main.PlayerInfo.ActionType = IActionType.PickUp;
                    }
                    else if (action is IActionSecondaryInteract interact)
                        interact.OnActionSecondary(Main.PlayerInfo);
                } 
            }
        } 
    }
    private static void HandleRaycast()
    { 
        Ray ray = Main.Camera.ScreenPointToRay(Input.mousePosition);
        
        if (MapCull.YCheck)
        {
            // Calculate the position in the camera's direction where y = yThreshold 
            float yThreshold = MapCull.YThreshold + 0.05f;
            Vector3 thresholdPoint = ray.origin + ray.direction * ((yThreshold - ray.origin.y) / ray.direction.y);
            
            if (NavMap.Get(Vector3Int.FloorToInt(thresholdPoint) + Vector3Int.down) != NavMap.Air)
            { 
                MouseLayer = Main.MaskMap;
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
