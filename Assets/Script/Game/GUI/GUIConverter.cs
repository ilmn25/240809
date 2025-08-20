using TMPro;
using UnityEngine;

public class GUIConverter : GUIStorage
{
    public ConverterInfo Info;
    protected override void ActionPrimaryDown()
    {
        if (Storage.List[CurrentSlotKey].Stack == 0 || Info.isFull()) return;
        Item item = Item.GetItem(Storage.List[CurrentSlotKey].ID);

        if (ItemRecipe.IsCraftable(item.StringID))
        {
            ItemRecipe.TakeIngredients(item.StringID);
            Info.Pending.Add(item.StringID);
        }
    }
    
    protected override void ActionSecondaryKey()
    {
        if (Storage.List[CurrentSlotKey].Stack == 0 || Info.isFull()) return;
        Item item = Item.GetItem(Storage.List[CurrentSlotKey].ID);

        if (ItemRecipe.IsCraftable(item.StringID))
        {
            ItemRecipe.TakeIngredients(item.StringID);
            Info.Pending.Add(item.StringID);
        }
    }
    
    protected override void SetInfoPanel(ItemSlot itemSlot)
    { 
        GUIMain.Cursor.Set(itemSlot.GetItemInfo(true));
        // GUIMain.InfoPanel.Set(itemSlot.GetItemInfo(true));
    }
}