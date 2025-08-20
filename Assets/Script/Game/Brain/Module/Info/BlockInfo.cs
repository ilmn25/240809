using System;
using UnityEngine;

[System.Serializable]
public class BlockInfo : DestructableInfo
{
    public ID blockID; 
 
    public override void OnDestroy(Projectile projectile)
    { 
        base.OnDestroy(projectile);
        World.SetBlock(Vector3Int.FloorToInt(position), Block.ConvertID(blockID));
        PlayerTerraformModule.Position.Remove(Vector3Int.FloorToInt(Machine.transform.position));
    }
}


[System.Serializable]
public class BreakBlockInfo : DestructableInfo
{ 
    public override void OnDestroy(Projectile projectile)
    { 
        base.OnDestroy(projectile);
        World.SetBlock(Vector3Int.FloorToInt(position));
        PlayerTerraformModule.Position.Remove(Vector3Int.FloorToInt(Machine.transform.position));
    }
}