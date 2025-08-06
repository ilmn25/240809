using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;


public class PlayerState : State
{
    private PlayerStatusModule _playerStatusModule;
    public override void OnInitialize()
    {
        _playerStatusModule = Machine.GetModule<PlayerStatusModule>();
        Inventory.SlotUpdate += EventSlotUpdate;
        AddState(new StateEmpty(), true);
        AddState(new EquipSwingState());
        AddState(new EquipShootState());
        AddState(new EquipSelectState());
    }

    private void HandleInput()
    {
        
        if (Machine.transform.position.y < -50)
        {
            MapCull.ForceRevertMesh(); 
            Machine.transform.position = new Vector3(Game.Player.transform.position.x , World.Inst.Bounds.y + 40, Game.Player.transform.position.z);
        }
         
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.Space))
            Game.GameState = GameState.Playing; 
        if (Input.GetKeyDown(KeyCode.M)) 
            Entity.SpawnPrefab("megumin", Vector3Int.FloorToInt(Machine.transform.position + Vector3.up));
        if (Input.GetKeyDown(KeyCode.L)) 
            Entity.SpawnPrefab("snare_flea", Vector3Int.FloorToInt(Machine.transform.position + Vector3.up));
    }
    
    public override void OnUpdateState()
    {
        HandleInput(); 
        if (!GUI.Active)
        {
            switch (Inventory.CurrentItemData?.Type)
            {
                case ItemType.Tool:
                    if (Inventory.CurrentItemData.MiningPower != 0 && 
                        Utility.isLayer(Control.MouseLayer, Game.IndexMap) &&
                        Scene.InPlayerBlockRange(Control.MousePosition, PlayerStatusModule.GetRange()))
                    {
                        PlayerTerraformModule.HandlePositionInfo(Control.MousePosition,  Control.MouseDirection); 
                        if (!_playerStatusModule.IsBusy && Control.Inst.ActionPrimary.Key()) 
                            PlayerTerraformModule.HandleMapBreak(); 
                    } 
                    if (!_playerStatusModule.IsBusy && 
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
                        Scene.InPlayerBlockRange(Control.MousePosition, PlayerStatusModule.GetRange()))
                    {
                        PlayerTerraformModule.HandlePositionInfo(Control.MousePosition, Control.MouseDirection);
                        if (!_playerStatusModule.IsBusy && Control.Inst.ActionSecondary.Key())
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
        Vector3 direction = _playerStatusModule.SpriteToolTrack.right;
        if (_playerStatusModule.SpriteToolTrack.lossyScale.x < 0f) 
            direction *= -1;
        direction.y = 0;
        direction.Normalize();
        
        // Offset the spawn origin based on that direction
        Projectile.Spawn(_playerStatusModule.SpriteToolTrack.position + 
                         direction * Inventory.CurrentItemData.HoldoutOffset,
            dest,
            Inventory.CurrentItemData.ProjectileInfo,
            HitboxType.Passive);
    }
    
    public void EventSlotUpdate()
    {
        if (Inventory.CurrentItemData == null)
            _playerStatusModule.SpriteTool.gameObject.SetActive(false);
        else
        {
            _playerStatusModule.SpriteTool.gameObject.SetActive(true);
            _playerStatusModule.SpriteToolRenderer.sprite = 
                Resources.Load<Sprite>($"texture/sprite/{Inventory.CurrentItemData.StringID}"); 
            _playerStatusModule.SpriteToolTrack.transform.localScale = Vector3.one * Inventory.CurrentItemData.Scale;
            SetState<EquipSelectState>();
        } 
    }
}
  