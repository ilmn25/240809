 
using UnityEngine;

public class PlayerMachine : EntityMachine, IHitBoxAttack, IActionSecondary
{
    private new PlayerInfo Info => GetModule<PlayerInfo>();
    public static Info CreateInfo()
    { 
        return new PlayerInfo()
        {  
            HitboxType = HitboxType.Friendly,
            TargetHitboxType = HitboxType.Passive,
            Storage = new Storage(27),
            HealthMax = 6,
            Defense = 0,
            Mana = 100,
            Sanity = 100,
            Hunger = 100,
            Stamina = 100,
            SpeedGround = 5,
            SpeedAir = 7,
            Iframes = 100, 
            PathAmount = 7000,
            MaxStuckCount = 100,
            AccelerationTime = 0.2f,
            DecelerationTime = 0.08f,
            DistAttack = 4,
            Gravity = -40f,
            JumpVelocity = 12f,
            DeathSfx = "player_die",
            HurtSfx = "player_hurt", 
        };
    }
    public override void OnStart()
    {   
        AddModule(new SpriteOrbitModule(transform)); 
        AddModule(new GroundAnimationModule()); 
        AddModule(new GroundMovementModule()); 
        AddModule(new GroundPathingModule()); 
        AddModule(new PlayerTerraformModule());  
        
        AddState(new MobAttackSwing());
        AddState(new MobAttackShoot());
        AddState(new MobChaseAction());
        AddState(new EquipSelectState());   
        AddState(new InContainerState()
        {
            Storage = Info.Storage
        });
    }

    public void OnActionSecondary(EntityMachine entityMachine)
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
         
        if (Info.Target && Info.ActionTarget == IActionTarget.Secondary && 
            (Input.GetKeyDown(KeyCode.A) ||
             Input.GetKeyDown(KeyCode.W) ||
             Input.GetKeyDown(KeyCode.S) ||
             Input.GetKeyDown(KeyCode.D) ||
             Input.GetKeyDown(KeyCode.Space)))
        {
            Info.PathingStatus = PathingStatus.Stuck;
            SetState<DefaultState>();
            Info.Target = null;
        } 
        
        if (Input.GetKeyDown(KeyCode.N))
            Entity.SpawnItem("brick", Vector3Int.FloorToInt(transform.position), 200);
        if (Input.GetKeyDown(KeyCode.M)) 
            Entity.Spawn("megumin", Vector3Int.FloorToInt(transform.position + Vector3.up));
        if (Input.GetKeyDown(KeyCode.L)) 
            Entity.Spawn("snare_flea", Vector3Int.FloorToInt(transform.position + Vector3.up));
        if (Input.GetKeyDown(KeyCode.K))
        {
            Entity.Spawn("chito", Vector3Int.FloorToInt(transform.position + Vector3.up));
            Entity.Spawn("yuuri", Vector3Int.FloorToInt(transform.position + Vector3.up));
        } 
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
                if (Info.Target && Info.ActionTarget == IActionTarget.Secondary)
                {
                    SetState<MobChaseAction>();
                }
                else if (!GUIMain.IsHover)
                {
                    switch (Info.Equipment?.Type)
                    {
                        case ItemType.Tool:
                            if (Info.Equipment.MiningPower != 0 &&
                                Utility.isLayer(Control.MouseLayer, Game.IndexMap) &&
                                Scene.InPlayerBlockRange(Control.MousePosition, Info.GetRange()))
                            {
                                PlayerTerraformModule.HandlePositionInfo(Control.MousePosition, Control.MouseDirection,
                                    true);
                                if (!Info.IsBusy && Control.Inst.ActionPrimary.Key())
                                    PlayerTerraformModule.HandleMapBreak();

                            }

                            if (Control.Inst.ActionPrimary.Key() || Control.Inst.DigUp.Key() ||
                                Control.Inst.DigDown.Key())
                            {
                                Attack();
                            }

                            break;

                        case ItemType.Block:
                            if (Utility.isLayer(Control.MouseLayer, Game.IndexMap) &&
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
            if (!Info.Target || !Info.Target.gameObject.activeSelf)
            {  
                if (Game.Player)
                {
                    Info.Target = Game.Player.transform;
                    Info.ActionTarget = IActionTarget.Follow;
                }
            } 
            SetState<MobChaseAction>();
        } 
    }
     
    public override void Attack()
    {
        if (Info.Equipment.ProjectileInfo.Ammo != null && 
            Inventory.Storage.GetAmount(Info.Equipment.ProjectileInfo.Ammo) == 0) return;
                    
        Info.AimPosition = Control.MouseTarget ?
            Control.MouseTarget.transform.position + Vector3.up * 0.55f :
            Control.MousePosition + Vector3.up * 0.15f; 
                    
        switch (Info.Equipment.Gesture)
        {
            case ItemGesture.Swing:
                SetState<MobAttackSwing>();
                break;
            case ItemGesture.Shoot:
                SetState<MobAttackShoot>();
                break;
        }
                    
        if (Info.Equipment.ProjectileInfo.Ammo != null) Inventory.RemoveItem(Info.Equipment.ProjectileInfo.Ammo);
    }
    
    public void OnDrawGizmos()
    {
        if (Camera.current != Camera.main)
            return;

        GetModule<GroundPathingModule>().DrawGizmos();
    }
} 
