using System;
using System.Collections.Generic;
using UnityEngine;

public class CraftData
{
    public int Stack;
    public Dictionary<string, int> Ingredients;
    public string[] Modifiers;

    public CraftData(Dictionary<string, int> ingredients, string[] modifiers = null, int stack = 1)
    {
        Stack = stack;
        Ingredients = ingredients;
        Modifiers = modifiers;
    }
}

namespace Script.World.Entity.Item
{
    public class CraftSingleton : MonoBehaviour
    {
        public static CraftSingleton Instance { get; private set; }  
        public static Dictionary<string, CraftData> _craftList = new Dictionary<string, CraftData>();

        void Awake()
        {
            Instance = this;
        }
 

        public static CraftData GetItem(string stringID)
        {
            return _craftList[stringID];
        }
        
        public static void AddCraftingDefinition(string stringID, Dictionary<string, int> ingredients)
        {
            _craftList.Add(stringID, new CraftData(ingredients));
        }

        bool IsCraftable (string stringID)
        {
            foreach (var ingredient in _craftList[stringID].Ingredients)
            {
                if (PlayerInventorySingleton.GetAmount(ingredient.Key) < ingredient.Value) return false;
            } 
            return true;
        }
        
        public void CraftItem(string stringID)
        {
            if (!IsCraftable(stringID)) return;

            foreach (var ingredient in _craftList[stringID].Ingredients)
            {
                PlayerInventorySingleton.RemoveItem(ingredient.Key, ingredient.Value);
            }
            
            InvSlotData craftedItem = new InvSlotData();
            
            craftedItem.SetItem(_craftList[stringID].Stack, stringID, null, false);
            if (GUICursorSingleton._cursorSlot.isEmpty() || GUICursorSingleton._cursorSlot.isSame(craftedItem))
            {
                GUICursorSingleton._cursorSlot.Add(craftedItem);
            }
            
            if (craftedItem.Stack > 0)
            { 
                PlayerInventorySingleton.AddItem(stringID);
            } 
        }
    }
}