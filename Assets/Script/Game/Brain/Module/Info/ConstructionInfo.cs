using UnityEngine;

[System.Serializable]
public class ConstructionInfo : SpriteStructureInfo
{
    public ID structureID; 

    public override string ToString()
    {
        return $"Building {Helper.ToDisplayName(structureID)} | {Health:0.#} Left";
    }

    public override void OnDestroy(MobInfo info)
    { 
        Entity.Spawn(structureID, Vector3Int.FloorToInt(Machine.transform.position));
    }
}