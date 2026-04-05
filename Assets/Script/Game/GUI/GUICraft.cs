using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

public class GUICraft : GUIStorage
{ 
    protected override void ActionPrimaryDown()
    {
        if (Storage.List[CurrentSlotKey].Stack == 0) return;
        Item item = Item.GetItem(Storage.List[CurrentSlotKey].ID);

        if (ItemRecipe.IsCraftable(item.ID))
        {
             ItemRecipe.CraftItem(item.ID, !Input.GetKey(KeyCode.LeftShift));
        }
    }
    
    protected override void ActionSecondaryKey()
    {
        if (Storage.List[CurrentSlotKey].Stack == 0) return;
        Item item = Item.GetItem(Storage.List[CurrentSlotKey].ID);

        if (ItemRecipe.IsCraftable(item.ID))
        {
            ItemRecipe.CraftItem(item.ID, false);
        }
    }

    protected override void SetInfoPanel(ItemSlot itemSlot)
    { 
        GUIMain.Cursor.SetItemSlotInfo(itemSlot, true);
    }
} 