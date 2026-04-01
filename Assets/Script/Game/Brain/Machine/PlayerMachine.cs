 
using System.Linq;
using UnityEditor;
using UnityEngine;
 
public class PlayerMachine : MobMachine, IActionSecondaryInteract
{
    private new PlayerInfo Info => GetModule<PlayerInfo>();

    private bool EnsureCompatibleToolForTarget()
    {
        if (Info.Target is not StructureInfo structureTarget) return true;

        if (Info.Storage.SetTool(structureTarget.operationType)) return true;

        Info.CancelTarget();
        return false;
    }

    public static Info CreateInfo()
    { 
        return new PlayerInfo()
        {  
            HitboxType = HitboxType.Friendly,
            targetHitboxType = HitboxType.Passive,
            Storage = new Storage(9)
            {
                Name = "Inventory"
            },
            HealthMax = 12,
            Defense = 0,
            Mana = 100,
            Sanity = 100,
            Stamina = 100,
            SpeedLogic = 5,
            SpeedGround = 5,
            SpeedAir = 6,
            Iframes = 100, 
            PathAmount = 6000,
            MaxStuckCount = 8,
            AccelerationTime = 0.3f,
            DecelerationTime = 0.1f,
            DistAttack = 2,
            Gravity = -40f,
            JumpVelocity = 12f,
            DeathSfx = SfxID.DeathPlayer,
            HitSfx = SfxID.HitPlayer, 
            CharSprite = ID.Chito 
        }; 
    }
    public override void OnStart()
    {    
        AddModule(new SpriteOrbitModule(transform)); 
        AddModule(new GroundAnimationModule()); 
        AddModule(new GroundMovementModule()); 
        AddModule(new GroundPathingModule()); 
        
        AddState(new DeadState());
        AddState(new MobAttackSwing());
        AddState(new MobAttackShoot());
        AddState(new MobChaseAction());
        AddState(new MobHit());
        AddState(new EquipSelectState());   
        AddState(new InContainerState()
        {
            Storage = Info.Storage
        });
    }

    public void OnActionSecondary(Info info)
    {
        if (IsCurrentState<InContainerState>())
            SetState<DefaultState>();
        else 
            SetState<InContainerState>();
    }
    
    private void HandleInput()
    {
        if (transform.position.y < -50)
        {
            MapCull.ForceRevertMesh(); 
            transform.position = new Vector3(Main.Player.transform.position.x , World.Inst.Bounds.y + 40, Main.Player.transform.position.z);
        }
         
        if (Info.Target != null && Info.ActionType is IActionType.PickUp or IActionType.Interact &&
            (Input.GetKeyDown(KeyCode.A) ||
             Input.GetKeyDown(KeyCode.W) ||
             Input.GetKeyDown(KeyCode.S) ||
             Input.GetKeyDown(KeyCode.D) ||
             Input.GetKeyDown(KeyCode.Space)))
        { 
            Info.CancelTarget();
        } 
    }
    
    public override void OnUpdate()
    {   
        Info.position = transform.position;

        if (Main.PlayerInfo == Info)
        { 
            Main.Player = gameObject;
            HandleInput();
            
            if (IsCurrentState<DefaultState>())
            {
                if (Info.Target != null && Info.ActionType is IActionType.PickUp or IActionType.Interact)
                { 
                    SetState<MobChaseAction>();
                }
                else if (!GUIMain.IsHover)
                {
                    switch (Info.Equipment?.Info.Type)
                    {
                        case ItemType.Tool: 
                            if (Control.Inst.ActionPrimary.Key())
                                Attack();
                            break;
                    }
                } 
            } 
        }
        else if (IsCurrentState<DefaultState>()) 
        { 
            // Validate current structure target if it exists
            if (Info.Target is StructureInfo && Info.ActionType is IActionType.Hit or IActionType.Dig)
            {
                EnsureCompatibleToolForTarget();
            }

            // If no target or only following player, search pending tasks
            if ((Info.Target == null || Info.Target == Main.PlayerInfo) && PlayerTask.Pending.Count != 0)
            {  
                foreach (StructureInfo structureInfo in PlayerTask.Pending)
                { 
                    if (Info.Storage.SetTool(structureInfo.operationType))
                    {
                        Info.Target = structureInfo;
                        Info.ActionType = IActionType.Hit; 
                        return;
                    } 
                } 
            }
            
            // If no task found, follow player
            if (Info.Target == null && Main.PlayerInfo.PlayerStatus == PlayerStatus.Active)
            {  
                Info.Target = Main.PlayerInfo;
                Info.ActionType = IActionType.Follow;
            } 
            
            SetState<MobChaseAction>();
        } 
    }
     
    public override void Attack()
    {
        if (Main.PlayerInfo != Info && !EnsureCompatibleToolForTarget())
        {
            return;
        }

        if (Info.Equipment == null)
        {
            Info.Target = null;
            return;
        }
        
        if (Info.Equipment.Info.ProjectileInfo != null)
        {
            if (Info.Equipment.Info.ProjectileInfo.Ammo != ID.Null && 
                Info.Storage.GetAmount(Info.Equipment.Info.ProjectileInfo.Ammo) == 0) return;
            Info.Storage.RemoveItem(Info.Equipment.Info.ProjectileInfo.Ammo);
        }

        base.Attack();
        
        if (Info.Equipment.Durability != -1)
        {
            Info.Equipment.Durability--;
            if (Info.Equipment.Durability == 0)
            {
                Info.Equipment.clear();
                Info.SetEquipment(null);
            } 
        }  
    }
    public void OnDrawGizmos()
    {
        if (Camera.current != Camera.main)
            return;

        GetModule<GroundPathingModule>().DrawGizmos();
    }
} 
