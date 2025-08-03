using Unity.VisualScripting;
using UnityEngine;

public class EquipState : State
{
    public Transform PlayerSprite;
    public Transform ToolSprite;
    EquipSwingState _equipSwingState;
    public void Action()
    {
        
    }
    public override void OnInitialize()
    {
        Inventory.SlotUpdate += EventSlotUpdate;
        PlayerSprite = Machine.transform.Find("sprite").transform.Find("char");
        ToolSprite = Machine.transform.Find("sprite").transform.Find("tool");  
        AddState(new EquipSwingState(), true);
        _equipSwingState = GetState<EquipSwingState>();
    }

    public override void OnUpdateState()
    {
        if (!PlayerStatus.IsBusy)
        {
            if (Control.Inst.ActionPrimary.KeyDown() || 
                Control.Inst.DigUp.KeyDown() ||
                Control.Inst.DigDown.KeyDown())
            {
                _equipSwingState.Use();
            }
        }  
    }

    public void EventSlotUpdate()
    {
        if (Inventory.CurrentItemData == null)
        {
            Utility.Log("Current item is null");
            ToolSprite.gameObject.SetActive(false);
        } 
        
        if (Inventory.CurrentItemData != null && Inventory.CurrentItemData.Type == ItemType.Block)
            PlayerTerraform.BlockStringID = Inventory.CurrentItem.StringID;
        else 
            PlayerTerraform.BlockStringID = null;

        if (Inventory.CurrentItemData != null && Inventory.CurrentItemData.Type == ItemType.Tool)
        {
            ToolSprite.gameObject.SetActive(true);
            ToolSprite.GetComponent<SpriteRenderer>().sprite = 
                Resources.Load<Sprite>($"texture/sprite/{Inventory.CurrentItemData.StringID}");
        }
        else
        {
            ToolSprite.gameObject.SetActive(false);
        } 
    }
}
  