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
                        if (Main.PlayerInfo.storage.GetAmount(ingredient.Key) < ingredient.Value) return false;
                } 
                return true;
        }

        public static void Build(Vector3Int position)
        {
                ConstructionInfo info = (ConstructionInfo)Entity.Spawn(ID.Construction, position);
                info.structureID = Target.StringID;
                info.Health = Target.Time;
                info.operationType = OperationType.Building;
                info.SfxHit = SfxID.HitMetal;
                info.SfxDestroy = SfxID.HitMetal;
                foreach (var ingredient in Target.Ingredients)
                {
                        Main.PlayerInfo.storage.RemoveItem(ingredient.Key, ingredient.Value);
                } 
                GUIMain.RefreshStorage();
        }
}