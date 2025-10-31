 
using System.Linq;
using UnityEditor;
using UnityEngine;
 
public class PlayerMachine : MobMachine, IActionSecondaryInteract
{
    private new PlayerInfo Info => GetModule<PlayerInfo>();
    public static Info CreateInfo()
    { 
        return new PlayerInfo()
        {  
            HitboxType = HitboxType.Friendly,
            targetHitboxType = HitboxType.Passive,
            storage = new Storage(9)
            {
                Name = "Inventory"
            },
            HealthMax = 12,
            Defense = 0,
            mana = 100,
            sanity = 100,
            stamina = 100,
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
            Storage = Info.storage
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
            if (Info.Target == null && Main.PlayerInfo.PlayerStatus == PlayerStatus.Active)
            {  
                Info.Target = Main.PlayerInfo;
                Info.ActionType = IActionType.Follow;
            } 
            
            if (Info.Target == Main.PlayerInfo && PlayerTask.Pending.Count != 0)
            {  
                foreach (StructureInfo structureInfo in PlayerTask.Pending)
                { 
                    if (Info.storage.SetTool(structureInfo.operationType))
                    {
                        Info.Target = structureInfo;
                        Info.ActionType = IActionType.Hit; 
                        return;
                    } 
                } 
            } 
            
            SetState<MobChaseAction>();
        } 
    }
     
    public override void Attack()
    {
        if (Info.Equipment == null)
        {
            Info.Target = null;
            return;
        }
        
        if (Info.Equipment.Info.ProjectileInfo != null)
        {
            if (Info.Equipment.Info.ProjectileInfo.Ammo != ID.Null && 
                Info.storage.GetAmount(Info.Equipment.Info.ProjectileInfo.Ammo) == 0) return;
            Info.storage.RemoveItem(Info.Equipment.Info.ProjectileInfo.Ammo);
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
