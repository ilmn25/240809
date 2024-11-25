using System;
using System.Collections.Generic;
using UnityEngine;

namespace Script.World.Entity.Item
{
    public class CraftStatic : MonoBehaviour
    {
        public static CraftStatic Instance { get; private set; }  
        private static Dictionary<string, Dictionary<string, int>> _craftList = new Dictionary<string, Dictionary<string, int>>();

        void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                craftItem("backroom");
            }
        }

        public static void AddCraftingDefinition(string stringID, Dictionary<string, int> ingredients)
        {
            _craftList.Add(stringID, ingredients);
        }

        bool IsCraftable (string stringID)
        {
            foreach (var ingredient in _craftList[stringID])
            {
                if (PlayerInventoryStatic.GetStackAmount(ingredient.Key) < ingredient.Value) return false;
            } 
            return true;
        }
        
        void craftItem(string stringID)
        {
            if (!IsCraftable(stringID)) return;
            PlayerInventoryStatic.AddItem(stringID);
        }
    }
}