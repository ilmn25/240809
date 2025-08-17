using System.Collections.Generic;
using UnityEngine;

public class StructureRecipe
{
        public static Dictionary<string, StructureRecipe> Dictionary = new Dictionary<string, StructureRecipe>();
        public static StructureRecipe Target;
        
        public string StringID;
        public int Time;
        public Dictionary<string, int> Ingredients;
        
        public static void AddRecipe(string stringID, int time, Dictionary<string, int> ingredients)
        {
                Dictionary.Add(stringID, new StructureRecipe()
                {
                        StringID = stringID,
                        Time = time,
                        Ingredients = ingredients, 
                });
        }
        
        public static bool IsCraftable (string stringID)
        {
                foreach (var ingredient in Dictionary[stringID].Ingredients)
                {
                        if (Game.PlayerInfo.Storage.GetAmount(ingredient.Key) < ingredient.Value) return false;
                } 
                return true;
        }

        public static void Build(Vector3Int position)
        {
                Entity.Spawn(Target!.StringID, position); 
                foreach (var ingredient in Target.Ingredients)
                {
                        Inventory.RemoveItem(ingredient.Key, ingredient.Value);
                } 
                GUIMain.RefreshStorage();
        }
}