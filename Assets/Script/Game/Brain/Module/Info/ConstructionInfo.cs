using UnityEngine;

[System.Serializable]
public class ConstructionInfo : StructureInfo
{
    public ID structureID; 
    public override void OnDestroy(MobInfo info)
    { 
        Entity.Spawn(structureID, Vector3Int.FloorToInt(Machine.transform.position));
    }
}