using System.Collections.Generic;
using UnityEngine;

namespace Script.World.Entity.Item
{
    public class CraftStatic : MonoBehaviour
    {
        public static CraftStatic Instance { get; private set; }  
        Dictionary<string, string[]> _craftList = new Dictionary<string, string[]>();

        void Awake()
        {
            Instance = this;
        }

        public void AddCraftingDefinition(string stringID, string[] ingredients)
        {
            _craftList.Add(stringID, ingredients);
        }

        void getCraftableList()
        {
            
        }

        void IsCraftable(string stringID)
        {
            
        }
        
        void craftItem(string stringID)
        {
            
        }
    }
}