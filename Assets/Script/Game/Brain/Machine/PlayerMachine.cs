 
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
            TargetHitboxType = HitboxType.Passive,
            Storage = new Storage(9)
            {
                Name = "Inventory"
            },
            HealthMax = 6,
            Defense = 0,
            Mana = 100,
            Sanity = 100,
            Hunger = 100,
            Stamina = 100,
            SpeedLogic = 4,
            SpeedGround = 4,
            SpeedAir = 5,
            Iframes = 100, 
            PathAmount = 7000,
            MaxStuckCount = 100,
            AccelerationTime = 0.2f,
            DecelerationTime = 0.08f,
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
        AddModule(new PlayerTerraformModule());  
        
        AddState(new DeadState());
        AddState(new MobAttackSwing());
        AddState(new MobAttackShoot());
        AddState(new MobChaseAction());
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
            transform.position = new Vector3(Game.Player.transform.position.x , World.Inst.Bounds.y + 40, Game.Player.transform.position.z);
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
        //
        // if (Input.GetKeyDown(KeyCode.N))
        // {
        //     Info.Storage.AddItem(ID.Log, 15);
        //     Info.Storage.AddItem(ID.Gravel, 15);
        //     Info.Storage.AddItem(ID.Flint, 15);
        //     Info.Storage.AddItem(ID.Sticks, 15);
        //     Info.Storage.AddItem(ID.Stake, 15);
        // } 
        // if (Input.GetKeyDown(KeyCode.M)) 
        //     Entity.Spawn(ID.Megumin, Vector3Int.FloorToInt(transform.position + Vector3.up));
        // if (Input.GetKeyDown(KeyCode.L)) 
        //     Entity.Spawn(ID.SnareFlea, Vector3Int.FloorToInt(transform.position + Vector3.up));
        // if (Input.GetKeyDown(KeyCode.K))
        // {
        //     Entity.Spawn(ID.Chito, Vector3Int.FloorToInt(transform.position + Vector3.up));
        //     Entity.Spawn(ID.Yuuri, Vector3Int.FloorToInt(transform.position + Vector3.up));
        // } 
    }
    
    public override void OnUpdate()
    {   
        Info.position = transform.position;
        if (Game.PlayerInfo == Info)
        { 
            Game.Player = gameObject;
            HandleInput();
            
            if (IsCurrentState<DefaultState>())
            {
                if (Info.Target != null && Info.ActionType is IActionType.PickUp or IActionType.Interact)
                { 
                    SetState<MobChaseAction>();
                }
                else if (!GUIMain.IsHover)
                {
                    switch (Info.Equipment?.Type)
                    {
                        case ItemType.Tool:
                            if (Info.Equipment.Name == "blueprint" &&
                                Helper.isLayer(Control.MouseLayer, Game.IndexMap) &&
                                Scene.InPlayerBlockRange(Control.MousePosition, Info.GetRange()))
                            {
                                PlayerTerraformModule.HandlePositionInfo(Control.MousePosition, Control.MouseDirection,
                                    true);
                                if (Control.Inst.ActionPrimary.Key())
                                {
                                    Audio.PlaySFX(SfxID.Item);
                                    PlayerTerraformModule.HandleMapBreak(); 
                                } 
                            }

                            if (Control.Inst.ActionPrimary.Key() || Control.Inst.DigUp.Key() ||
                                Control.Inst.DigDown.Key())
                            {
                                Attack();
                            }

                            break;

                        case ItemType.Block:
                            if (Helper.isLayer(Control.MouseLayer, Game.IndexMap) &&
                                Scene.InPlayerBlockRange(Control.MousePosition, Info.GetRange()))
                            {
                                PlayerTerraformModule.HandlePositionInfo(Control.MousePosition, Control.MouseDirection,
                                    false);
                                if (Control.Inst.ActionSecondary.Key())
                                {
                                    SetState<MobAttackSwing>();
                                    PlayerTerraformModule.HandleMapPlace();
                                }
                            }

                            break;
                    }
                } 
            } 
        }
        else if (IsCurrentState<DefaultState>()) 
        { 
            if (Info.Target == null && Game.PlayerInfo.PlayerStatus == PlayerStatus.Active)
            {  
                if (Game.PlayerInfo.CombatCooldown < 0)
                {
                    Info.Target = Game.PlayerInfo;
                    Info.ActionType = IActionType.Follow;
                }
            } 
            
            if (Info.Target == Game.PlayerInfo && PlayerTask.Pending.Count != 0)
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
            
            SetState<MobChaseAction>();
        } 
    }
     
    public override void Attack()
    {
        if (Info.Equipment.ProjectileInfo != null)
        {
            if (Info.Equipment.ProjectileInfo.Ammo != ID.Null && 
                Info.Storage.GetAmount(Info.Equipment.ProjectileInfo.Ammo) == 0) return;
            Info.Storage.RemoveItem(Info.Equipment.ProjectileInfo.Ammo);
        } 
        // if (Info.Eq != -1) durabilty--; and delete
        base.Attack();
    }
    public void OnDrawGizmos()
    {
        if (Camera.current != Camera.main)
            return;

        GetModule<GroundPathingModule>().DrawGizmos();
    }
} 
