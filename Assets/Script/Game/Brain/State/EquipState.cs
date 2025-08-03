using Unity.VisualScripting;
using UnityEngine;

public class EquipIdleState : State { }

public class EquipState : State
{
    private Transform _toolSprite; 
    public override void OnInitialize()
    {
        Inventory.SlotUpdate += EventSlotUpdate;
        _toolSprite = Machine.transform.Find("sprite").transform.Find("tool");  
        AddState(new EquipIdleState(), true);
        AddState(new EquipSwingState());
    }

    public override void OnUpdateState()
    { 
        if (!PlayerStatusModule.IsBusy && !GUI.Active)
        {
            switch (Inventory.CurrentItemData?.Type)
            {
                case ItemType.Tool:
                    if (Control.Inst.ActionPrimary.KeyDown() ||
                        Control.Inst.DigUp.KeyDown() ||
                        Control.Inst.DigDown.KeyDown())
                    {
                        Animate();
                        Attack();
                    }

                    if (Inventory.CurrentItemData.MiningPower != 0 && 
                        Utility.isLayer(Control.LayerMask, Game.IndexMap) &&
                        Scene.InPlayerBlockRange(Control.Position, PlayerStatusModule.GetRange()))
                    {
                        PlayerTerraformModule.HandlePositionInfo(Control.Position,  Control.Direction); 
                        if (Control.Inst.ActionPrimary.KeyDown()) PlayerTerraformModule.HandleMapBreak(); 
                    } 
                    break;
                
                case ItemType.Block:
                    if (GUI.Active) return;
                    if (Utility.isLayer(Control.LayerMask, Game.IndexMap) &&
                        Scene.InPlayerBlockRange(Control.Position, PlayerStatusModule.GetRange()))
                    {
                        PlayerTerraformModule.HandlePositionInfo(Control.Position, Control.Direction);
                        if (Control.Inst.ActionSecondary.KeyDown())
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
        if (Inventory.CurrentItemData.Type != ItemType.Tool) return;
        
        Collider[] hitColliders = Physics.OverlapBox(Machine.transform.position, Vector3.one * PlayerStatusModule.GetRange(), Quaternion.identity, Game.MaskEntity);
        IHitBox target;
        foreach (Collider collider in hitColliders)
        {
            target = collider.gameObject.GetComponent<IHitBox>();
            if (target == null) continue;
            target.OnHit(); 
        }
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
  