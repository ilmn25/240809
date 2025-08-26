using TMPro;
using UnityEngine;

public class GUIConverter : GUIStorage
{
    public ConverterInfo Info;
    protected override void ActionPrimaryDown()
    {
        if (Storage.List[CurrentSlotKey].Stack == 0 || Info.isFull()) return;
        Item item = Item.GetItem(Storage.List[CurrentSlotKey].ID);

        if (ItemRecipe.IsCraftable(item.ID))
        {
            ItemRecipe.TakeIngredients(item.ID);
            Info.Pending.Add(item.ID);
        }
    }
    
    protected override void ActionSecondaryKey()
    {
        if (Storage.List[CurrentSlotKey].Stack == 0 || Info.isFull()) return;
        Item item = Item.GetItem(Storage.List[CurrentSlotKey].ID);

        if (ItemRecipe.IsCraftable(item.ID))
        {
            ItemRecipe.TakeIngredients(item.ID);
            Info.Pending.Add(item.ID);
        }
    }
    
    protected override void SetInfoPanel(ItemSlot itemSlot)
    { 
        GUIMain.Cursor.Set(itemSlot, true);
        // GUIMain.InfoPanel.Set(itemSlot.GetItemInfo(true));
    }
}