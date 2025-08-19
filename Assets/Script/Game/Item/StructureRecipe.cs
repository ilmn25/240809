using System.Collections.Generic;
using UnityEngine;

public class StructureRecipe
{
        public static Dictionary<ID, StructureRecipe> Dictionary = new Dictionary<ID, StructureRecipe>();
        public static StructureRecipe Target;
        
        public ID StringID;
        public int Time;
        public Dictionary<ID, int> Ingredients;
        
        public static void AddRecipe(ID stringID, int time, Dictionary<ID, int> ingredients)
        {
                Dictionary.Add(stringID, new StructureRecipe()
                {
                        StringID = stringID,
                        Time = time,
                        Ingredients = ingredients, 
                });
        }
        
        public static bool IsCraftable (ID stringID)
        {
                foreach (var ingredient in Dictionary[stringID].Ingredients)
                {
                        if (Game.PlayerInfo.Storage.GetAmount(ingredient.Key) < ingredient.Value) return false;
                } 
                return true;
        }

        public static void Build(Vector3Int position)
        {
                ConstructionInfo info = (ConstructionInfo)Entity.Spawn(ID.Construction, position);
                info.structureID = Target.StringID;
                info.Health = Target.Time;
                info.operationType = OperationType.Building;
                info.SfxHit = "dig_metal";
                info.SfxDestroy = "dig_metal";
                foreach (var ingredient in Target.Ingredients)
                {
                        Game.PlayerInfo.Storage.RemoveItem(ingredient.Key, ingredient.Value);
                } 
                GUIMain.RefreshStorage();
        }
}