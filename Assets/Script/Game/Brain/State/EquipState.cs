using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;


public class EquipState : State
{
    private PlayerStatusModule _playerStatusModule;
    private Transform _toolTrack; 
    private Transform _toolSprite; 
    public override void OnInitialize()
    {
        _playerStatusModule = Machine.GetModule<PlayerStatusModule>();
        Inventory.SlotUpdate += EventSlotUpdate;
        _toolTrack = Machine.transform.Find("sprite").transform.Find("tool_track");  
        _toolSprite = _toolTrack.transform.Find("tool");  
        AddState(new StateEmpty(), true);
        AddState(new EquipSwingState());
        AddState(new EquipShootState());
    }

    public override void OnUpdateState()
    { 
        Vector3 mousePos = Input.mousePosition;
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
        Vector2 direction = mousePos - screenCenter;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (angle > 90)
            angle = 180 - angle;
        else if (angle < -90)
            angle = -angle - 180;

        float z = Mathf.Lerp(-0.45f, 0.45f, (angle + 90) / 180);
        if (z is > 0f and <= 0.12f)
            z = 0.12f;
        else if (z is < 0f and >= -0.11f)
            z = -0.11f;
        // float angleX = (Mathf.Lerp(0, 90, Math.Abs(angle) / 45) + 360) % 360;
        // Normalize angle to 0–360
        _toolTrack.localPosition = new Vector3(0, 0.3f, z);
        _toolTrack.localRotation = Quaternion.Euler(80, 0, (angle + 360) % 360);
        
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
                        if (!_playerStatusModule.IsBusy && Control.Inst.ActionPrimary.KeyDown()) 
                            PlayerTerraformModule.HandleMapBreak(); 
                    } 
                    if (!_playerStatusModule.IsBusy && 
                        (Control.Inst.ActionPrimary.Key() ||
                         Control.Inst.DigUp.KeyDown() ||
                         Control.Inst.DigDown.KeyDown()))
                    {
                        Attack();
                        Animate(); 
                    }
 
                    break;
                
                case ItemType.Block: 
                    if (Utility.isLayer(Control.MouseLayer, Game.IndexMap) &&
                        Scene.InPlayerBlockRange(Control.MousePosition, PlayerStatusModule.GetRange()))
                    {
                        PlayerTerraformModule.HandlePositionInfo(Control.MousePosition, Control.MouseDirection);
                        if (!_playerStatusModule.IsBusy && Control.Inst.ActionSecondary.KeyDown())
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
        Vector3 direction = _toolTrack.transform.right;
        if (_toolTrack.lossyScale.x < 0f) 
            direction *= -1;
        direction.y = 0;
        direction.Normalize();
        
        // Offset the spawn origin based on that direction
        Projectile.Spawn(_toolTrack.transform.position + direction * Inventory.CurrentItemData.HoldoutOffset,
            dest,
            Inventory.CurrentItemData.ProjectileInfo,
            HitboxType.Passive);
    }

    
    public void EventSlotUpdate()
    {
        if (Inventory.CurrentItemData == null)
            _toolSprite.gameObject.SetActive(false);
        else
        {
            _toolSprite.gameObject.SetActive(true);
            _toolSprite.GetComponent<SpriteRenderer>().sprite = 
                Resources.Load<Sprite>($"texture/sprite/{Inventory.CurrentItemData.StringID}"); 
            _toolSprite.transform.localScale = Vector3.one * Inventory.CurrentItemData.Scale;
        } 
    }
}
  