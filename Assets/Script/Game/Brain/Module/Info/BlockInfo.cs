using System;
using UnityEngine;

[System.Serializable]
public class BlockInfo : StructureInfo
{
    public ID blockID; 
 
    public override void OnDestroy(MobInfo info)
    { 
        World.SetBlock(Vector3Int.FloorToInt(position), Block.ConvertID(blockID));
        PlayerTerraformModule.Position.Remove(Vector3Int.FloorToInt(Machine.transform.position));
    }
}


[System.Serializable]
public class BreakBlockInfo : StructureInfo
{ 
    public override void OnDestroy(MobInfo info)
    { 
        World.SetBlock(Vector3Int.FloorToInt(position));
        PlayerTerraformModule.Position.Remove(Vector3Int.FloorToInt(Machine.transform.position));
    }
}