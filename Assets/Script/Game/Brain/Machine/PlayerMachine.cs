 
using UnityEngine;

public class PlayerMachine : EntityMachine, IHitBox
{
    private PlayerInfo _info;
    public override void OnStart()
    {   
        
        AddModule(new PlayerInfo()
        { 
            Equipment = Inventory.CurrentItemData,
            HitboxType = HitboxType.Friendly,
            TargetHitboxType = HitboxType.Passive,
            HealthMax = PlayerData.Inst.health,
            Defense = 0,
            Mana = PlayerData.Inst.mana,
            Sanity = PlayerData.Inst.sanity,
            Hunger = PlayerData.Inst.hunger,
            Stamina = PlayerData.Inst.stamina,
            SpeedGround = 4,
            SpeedAir = 6,
            AccelerationTime = 0.2f,
            DecelerationTime = 0.08f,
            Gravity = -40f,
            JumpVelocity = 12f,
            DeathSfx = "player_die",
            HurtSfx = "player_hurt", 
        });
        AddModule(new SpriteOrbitModule(transform)); 
        AddModule(new GroundAnimationModule()); 
        AddModule(new GroundMovementModule()); 
        AddModule(new PlayerTerraformModule());  
        
        _info = GetModule<PlayerInfo>();
        Inventory.SlotUpdate += EventSlotUpdate;
        AddState(new MobAttackSwing());
        AddState(new MobAttackShoot());
        AddState(new EquipSelectState());
    }

    private void HandleInput()
    {
        // Debug.Log(Info.SpeedCurrent);
        if (transform.position.y < -50)
        {
            MapCull.ForceRevertMesh(); 
            transform.position = new Vector3(Game.Player.transform.position.x , World.Inst.Bounds.y + 40, Game.Player.transform.position.z);
        }
          
        if (Input.GetKeyDown(KeyCode.N))
        {
            Entity.SpawnItem("brick", Vector3Int.FloorToInt(transform.position), 200);
        }
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
        HandleInput();
        if (GUIMain.IsHover || !IsCurrentState<DefaultState>()) return;
        if (Inventory.CurrentItemData != null) Info.Equipment = Inventory.CurrentItemData;
        switch (Inventory.CurrentItemData?.Type)
        {
            case ItemType.Tool:
                if (Inventory.CurrentItemData.MiningPower != 0 && 
                    Utility.isLayer(Control.MouseLayer, Game.IndexMap) &&
                    Scene.InPlayerBlockRange(Control.MousePosition, _info.GetRange()))
                {
                    PlayerTerraformModule.HandlePositionInfo(Control.MousePosition,  Control.MouseDirection); 
                    if (!_info.IsBusy && Control.Inst.ActionPrimary.Key()) 
                        PlayerTerraformModule.HandleMapBreak(); 
                     
                } 
                if (!_info.IsBusy && 
                    (Control.Inst.ActionPrimary.Key() ||
                     Control.Inst.DigUp.Key() ||
                     Control.Inst.DigDown.Key()))
                {
                    if (Info.Equipment.Ammo != null && 
                        Inventory.Storage.GetAmount(Info.Equipment.Ammo) == 0) return;
                    
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
                    
                    if (Info.Equipment.Ammo != null) Inventory.RemoveItem(Info.Equipment.Ammo);
                }

                break;
            
            case ItemType.Block: 
                if (Utility.isLayer(Control.MouseLayer, Game.IndexMap) &&
                    Scene.InPlayerBlockRange(Control.MousePosition, _info.GetRange()))
                {
                    PlayerTerraformModule.HandlePositionInfo(Control.MousePosition, Control.MouseDirection);
                    if (!_info.IsBusy && Control.Inst.ActionSecondary.Key())
                    {
                        SetState<MobAttackSwing>();
                        PlayerTerraformModule.HandleMapPlace();
                    }
                }
                break;
             
        } 
    }
  
 
    
    public void EventSlotUpdate()
    {
        if (Inventory.CurrentItemData == null)
            _info.SpriteTool.gameObject.SetActive(false);
        else
        {
            _info.SpriteTool.gameObject.SetActive(true);
            _info.SpriteToolRenderer.sprite = 
            _info.SpriteToolRenderer.sprite = Cache.LoadSprite("sprite/" + Inventory.CurrentItemData.StringID);
            _info.SpriteToolTrack.transform.localScale = Vector3.one * Inventory.CurrentItemData.Scale;
            SetState<EquipSelectState>();
        } 
    }
} 
