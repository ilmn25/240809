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

        if (item.Type == ItemType.Structure && StructureRecipe.IsCraftable(item.StringID))
        {
            StructureRecipe.Target =  StructureRecipe.Dictionary[item.StringID];
        } 
        else if (item.Type != ItemType.Structure && ItemRecipe.IsCraftable(item.StringID))
        {
             ItemRecipe.CraftItem(item.StringID, !Input.GetKey(KeyCode.LeftShift));
        }
    }
    
    protected override void ActionSecondaryKey()
    {
        if (Storage.List[CurrentSlotKey].Stack == 0) return;
        Item item = Item.GetItem(Storage.List[CurrentSlotKey].ID);

        if (item.Type != ItemType.Structure && ItemRecipe.IsCraftable(item.StringID))
        {
            ItemRecipe.CraftItem(item.StringID);
        }
    }

    protected override void SetInfoPanel(ItemSlot itemSlot)
    { 
        GUIMain.Cursor.Set(itemSlot, true);
        // GUIMain.InfoPanel.Set(itemSlot.GetItemInfo(true));
    }
}

// public class GUICraft 
// {
//     private static string _stringID;
//      
//     private static CoroutineTask _decelerateCoroutine;
//     private static float _scrollSpeed;
//     private static RectTransform _craftRect;
//     private const int SlotSize = 30;
//     private static readonly Vector2 Margin = new Vector2(10, 10); 
//
//     public static void Initialize()
//     {
//         _craftRect = Game.GUIInvCrafting.GetComponent<RectTransform>();
//         
//         int count = 1;
//         foreach (var recipe in ItemRecipe.Dictionary)
//         {
//             GameObject slot = Object.Instantiate(Resources.Load<GameObject>($"prefab/gui_item_slot"), Game.GUIInvCrafting.transform, false);
//      
//             RectTransform slotRectTransform = slot.GetComponent<RectTransform>();
//             slotRectTransform.sizeDelta = new Vector2(SlotSize, SlotSize);
//             slotRectTransform.anchoredPosition = new Vector2(count * (SlotSize + Margin.x), 0);
//             slot.AddComponent<GUICraftSlotModule>().Initialize(recipe.Key);
//             count++;
//         }
//     }
//     
//     public static void Update()
//     {
//         if (Control.Inst.ActionPrimary.KeyDown() && _stringID != null)
//         {
//             ItemRecipe.CraftItem(_stringID); 
//         } 
//     }
//     
//     public static void SetInfoPanel(string stringID = null)
//     {
//         _stringID = stringID;
//         
//         if (stringID == null)
//         {  
//             GUIMain.Cursor.Set(); 
//             return;
//         }
//
//         Item itemData = Item.GetItem(stringID);
//         ItemRecipe itemRecipeData = ItemRecipe.GetRecipe(stringID);
//         
//         string text = itemData.Name + " (" + itemRecipeData.Stack + ")\n";
//         foreach (var ingredient in itemRecipeData.Ingredients)
//         {
//             text += $"{ingredient.Key} ({ingredient.Value})\n";
//         }
//         text += itemData.Description + "\n";
//         
//         GUIMain.Cursor.Set(text);
//     }
//  
//     public static void HandleScrollInput(float input)
//     {
//         _scrollSpeed = input * 10000;
//
//         if (_decelerateCoroutine != null && _decelerateCoroutine.Running)
//             _decelerateCoroutine.Stop();
//
//         _decelerateCoroutine = new CoroutineTask(Decelerate());
//     }
//
//     private static IEnumerator Decelerate()
//     {
//         while (Mathf.Abs(_scrollSpeed) > 0.1f)
//         {
//             _craftRect.anchoredPosition += new Vector2(_scrollSpeed * Time.deltaTime, 0);
//             _scrollSpeed = Mathf.Lerp(_scrollSpeed, 0, Time.deltaTime * 5);
//             yield return null;
//         }
//         _scrollSpeed = 0;
//     }
// }
