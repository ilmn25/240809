 
using UnityEngine;

public class PlayerMachine : BasicMachine, IHitBox
{
    private new PlayerInfo Info;
    public override void OnStart()
    { 
        PlayerData.Load(); 
        AddModule(new PlayerInfo()
        {
            HitboxType = HitboxType.Friendly,
            HealthMax = PlayerData.Inst.health,
            Defense = 0,
            Mana = PlayerData.Inst.mana,
            Sanity = PlayerData.Inst.sanity,
            Hunger = PlayerData.Inst.hunger,
            Stamina = PlayerData.Inst.stamina,
            SpeedGround = PlayerData.Inst.speed,
            DeathSfx = "player_die",
            HurtSfx = "player_hurt"
        });
        AddModule(new PlayerMovementModule()); 
        AddModule(new PlayerAnimationModule()); 
        AddModule(new PlayerTerraformModule());  
        
        Info = GetModule<PlayerInfo>();
        Inventory.SlotUpdate += EventSlotUpdate;
        AddState(new EquipSwingState());
        AddState(new EquipShootState());
        AddState(new EquipSelectState());
    }

    private void HandleInput()
    {
        
        if (transform.position.y < -50)
        {
            MapCull.ForceRevertMesh(); 
            transform.position = new Vector3(Game.Player.transform.position.x , World.Inst.Bounds.y + 40, Game.Player.transform.position.z);
        }
         
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.Space))
            Info.PlayerStatus = PlayerStatus.Active; 
        if (Input.GetKeyDown(KeyCode.M)) 
            Entity.SpawnPrefab("megumin", Vector3Int.FloorToInt(transform.position + Vector3.up));
        if (Input.GetKeyDown(KeyCode.L)) 
            Entity.SpawnPrefab("snare_flea", Vector3Int.FloorToInt(transform.position + Vector3.up));
        if (Input.GetKeyDown(KeyCode.K)) 
            Entity.SpawnPrefab("chito", Vector3Int.FloorToInt(transform.position + Vector3.up));
    }
    
    public override void OnUpdate()
    {
        HandleInput(); 
        if (!GUI.Active)
        {
            switch (Inventory.CurrentItemData?.Type)
            {
                case ItemType.Tool:
                    if (Inventory.CurrentItemData.MiningPower != 0 && 
                        Utility.isLayer(Control.MouseLayer, Game.IndexMap) &&
                        Scene.InPlayerBlockRange(Control.MousePosition, Info.GetRange()))
                    {
                        PlayerTerraformModule.HandlePositionInfo(Control.MousePosition,  Control.MouseDirection); 
                        if (!Info.IsBusy && Control.Inst.ActionPrimary.Key()) 
                            PlayerTerraformModule.HandleMapBreak(); 
                    } 
                    if (!Info.IsBusy && 
                        (Control.Inst.ActionPrimary.Key() ||
                         Control.Inst.DigUp.Key() ||
                         Control.Inst.DigDown.Key()))
                    {
                        if (Inventory.CurrentItemData.Ammo != null && 
                            Inventory.GetAmount(Inventory.CurrentItemData.Ammo) == 0) return;
                        Attack();
                        Animate(); 
                        if (Inventory.CurrentItemData.Ammo != null) Inventory.RemoveItem(Inventory.CurrentItemData.Ammo);
                    }
 
                    break;
                
                case ItemType.Block: 
                    if (Utility.isLayer(Control.MouseLayer, Game.IndexMap) &&
                        Scene.InPlayerBlockRange(Control.MousePosition, Info.GetRange()))
                    {
                        PlayerTerraformModule.HandlePositionInfo(Control.MousePosition, Control.MouseDirection);
                        if (!Info.IsBusy && Control.Inst.ActionSecondary.Key())
                        {
                            Animate();
                            PlayerTerraformModule.HandleMapPlace();
                        }
                    }
                    break;
            } 
        }
    }
 

    private void Animate()
    {
        switch (Inventory.CurrentItemData.Gesture)
        {
            case ItemGesture.Swing:
                SetState<EquipSwingState>();
                break;
            case ItemGesture.Shoot:
                SetState<EquipShootState>();
                break;
        }
    }

    private void Attack()
    { 
        Vector3 dest = Control.MouseTarget ?
            Control.MouseTarget.transform.position + Vector3.up * 0.55f :
            Control.MousePosition + Vector3.up * 0.15f;
        
        // Use ToolTrack's global facing direction, flattened to horizontal
        Vector3 direction = Info.SpriteToolTrack.right;
        if (Info.SpriteToolTrack.lossyScale.x < 0f) 
            direction *= -1;
        direction.y = 0;
        direction.Normalize();
        
        // Offset the spawn origin based on that direction
        Projectile.Spawn(Info.SpriteToolTrack.position + 
                         direction * Inventory.CurrentItemData.HoldoutOffset,
            dest,
            Inventory.CurrentItemData.ProjectileInfo,
            HitboxType.Passive);
    }
    
    public void EventSlotUpdate()
    {
        if (Inventory.CurrentItemData == null)
            Info.SpriteTool.gameObject.SetActive(false);
        else
        {
            Info.SpriteTool.gameObject.SetActive(true);
            Info.SpriteToolRenderer.sprite = 
            Info.SpriteToolRenderer.sprite = Cache.LoadSprite("sprite/" + Inventory.CurrentItemData.StringID);
            Info.SpriteToolTrack.transform.localScale = Vector3.one * Inventory.CurrentItemData.Scale;
            SetState<EquipSelectState>();
        } 
    }
} 
