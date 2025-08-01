using Unity.VisualScripting;

public class InventoryState : State
{
    public override void OnInitialize()
    {
        Inventory.SlotUpdate += HandleItemUpdate;
    }

    public override void OnExitState()
    {
        Inventory.SlotUpdate -= HandleItemUpdate;
    }

    public override void OnEnterState()
    {
        AddState(new ItemEmptyState(), true);
        AddState(new ItemBlockState());
        AddState(new ItemToolState());
    }
    public void HandleItemUpdate()
    {
        if (Inventory.CurrentItem.Stack == 0)
        {
            SetState<ItemEmptyState>();
            return;
        }
        switch (Item.GetItem(Inventory.CurrentItem.StringID).Type)
        { 
            case ItemType.Block:
                SetState<ItemBlockState>();
                break;
            case ItemType.Tool:
                SetState<ItemToolState>();
                break;
            default:
                SetState<ItemEmptyState>();
                break;
        }
    }
}

public class ItemEmptyState : State
{
    public override void OnEnterState()
    {
        Machine.transform.Find("sprite").transform.Find("tool").gameObject.SetActive(false);
    }
}

public class ItemFurniture : State
{
    public string stringID; 

    public override void OnUpdateState()
    { 
        stringID = Inventory.CurrentItem.StringID;
    }
}

public class ItemBlockState : State
{ 
    public override void OnUpdateState()
    {  
        PlayerTerraform.BlockStringID = Inventory.CurrentItem.StringID;
    }

    public override void OnExitState()
    {
        PlayerTerraform.BlockStringID = null;
    }
}