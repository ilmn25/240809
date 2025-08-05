using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;


public class EquipState : State
{
    private PlayerStatusModule _playerStatusModule;
    private Transform _toolSprite; 
    private Transform _toolTip; 
    public override void OnInitialize()
    {
        _playerStatusModule = Machine.GetModule<PlayerStatusModule>();
        Inventory.SlotUpdate += EventSlotUpdate;
        _toolSprite = Machine.transform.Find("sprite").transform.Find("tool");  
        _toolTip = _toolSprite.transform.Find("tip");  
        AddState(new StateEmpty(), true);
        AddState(new EquipSwingState());
        AddState(new EquipShootState());
    }

    public override void OnUpdateState()
    { 
        if (!GUI.Active)
        {
            switch (Inventory.CurrentItemData?.Type)
            {
                case ItemType.Tool:
                    if (Inventory.CurrentItemData.MiningPower != 0 && 
                        Utility.isLayer(Control.LayerMask, Game.IndexMap) &&
                        Scene.InPlayerBlockRange(Control.Position, PlayerStatusModule.GetRange()))
                    {
                        PlayerTerraformModule.HandlePositionInfo(Control.Position,  Control.Direction); 
                        if (!_playerStatusModule.IsBusy && Control.Inst.ActionPrimary.KeyDown()) 
                            PlayerTerraformModule.HandleMapBreak(); 
                    } 
                    if (!_playerStatusModule.IsBusy && 
                        (Control.Inst.ActionPrimary.KeyDown() ||
                         Control.Inst.DigUp.KeyDown() ||
                         Control.Inst.DigDown.KeyDown()))
                    {
                        Attack();
                        Animate(); 
                    }
 
                    break;
                
                case ItemType.Block: 
                    if (Utility.isLayer(Control.LayerMask, Game.IndexMap) &&
                        Scene.InPlayerBlockRange(Control.Position, PlayerStatusModule.GetRange()))
                    {
                        PlayerTerraformModule.HandlePositionInfo(Control.Position, Control.Direction);
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
        Projectile.Spawn(_toolTip.transform.position, Control.Position, 
            Inventory.CurrentItemData.ProjectileInfo, HitboxType.Passive);
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
  