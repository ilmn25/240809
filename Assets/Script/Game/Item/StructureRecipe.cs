using System.Collections.Generic;

public class StructureRecipe
{
        public static Dictionary<string, StructureRecipe> Dictionary = new Dictionary<string, StructureRecipe>();
        
        public string StringID;
        public int Time;
        public Dictionary<string, int> Ingredients;

        static StructureRecipe()
        {
                AddRecipe("chest", 100, new Dictionary<string, int>()
                {
                        {"wood", 15},
                });
        }
        
        public static void AddRecipe(string stringID, int time, Dictionary<string, int> ingredients)
        {
                Dictionary.Add(stringID, new StructureRecipe()
                {
                        StringID = stringID,
                        Time = time,
                        Ingredients = ingredients, 
                });
        }
        
        private static bool IsCraftable (string stringID)
        {
                foreach (var ingredient in Dictionary[stringID].Ingredients)
                {
                        if (Inventory.Storage.GetAmount(ingredient.Key) < ingredient.Value) return false;
                } 
                return true;
        }
}