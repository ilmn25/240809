 

using UnityEngine;

public class ItemToolState : State
{  
    ToolSwingState _toolSwingState;
    public Transform PlayerSprite;
    public Transform ToolSprite;
    
    public override void OnInitialize()
    {   
        PlayerSprite = Machine.transform.Find("sprite").transform.Find("char");
        ToolSprite = Machine.transform.Find("sprite").transform.Find("tool");  
        AddState(new ToolSwingState(),true); 
        _toolSwingState = GetState<ToolSwingState>();
    }
    
    private void OnSlotUpdate() { 
        ToolSprite.GetComponent<SpriteRenderer>().sprite = 
            Resources.Load<Sprite>($"texture/sprite/{Inventory.CurrentItemData.StringID}");  
    }
    public override void OnEnterState()
    {  
        ToolSprite.gameObject.SetActive(true);
        ToolSprite.GetComponent<SpriteRenderer>().sprite = 
            Resources.Load<Sprite>($"texture/sprite/{Inventory.CurrentItemData.StringID}"); 
        Inventory.SlotUpdate += OnSlotUpdate;
    } 
    public override void OnExitState()
    {
        Inventory.SlotUpdate -= OnSlotUpdate;
        ToolSprite.gameObject.SetActive(false);
    } 

    public override void OnUpdateState()
    { 
        if (!PlayerStatus.IsBusy)
        {
            if (Control.Inst.ActionPrimary.KeyDown() || 
                Control.Inst.DigUp.KeyDown() ||
                Control.Inst.DigDown.KeyDown())
            {
                _toolSwingState.Use();
            }
        }  
    }
}