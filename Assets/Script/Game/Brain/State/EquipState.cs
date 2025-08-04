using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class EquipIdleState : State { }

public class EquipState : State
{
    private PlayerStatusModule _playerStatusModule;
    private Transform _toolSprite; 
    public override void OnInitialize()
    {
        _playerStatusModule = Machine.GetModule<PlayerStatusModule>();
        Inventory.SlotUpdate += EventSlotUpdate;
        _toolSprite = Machine.transform.Find("sprite").transform.Find("tool");  
        AddState(new EquipIdleState(), true);
        AddState(new EquipSwingState());
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
                        Animate();
                        Attack();
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
        }
    }

    private void Attack()
    {
        Projectile.Spawn(Machine.transform.position, Control.Position, 
            Inventory.CurrentItemData.ProjectileInfo, ProjectileTarget.Passive);
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
        } 
    }
}
  