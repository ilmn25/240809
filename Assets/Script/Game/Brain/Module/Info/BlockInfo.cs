using System;
using UnityEngine;

[System.Serializable]
public class BlockInfo : DestructableInfo
{
    public string blockID; 
 
    
    public override void OnDestroy(Projectile projectile)
    { 
        base.OnDestroy(projectile);
        World.SetBlock(Vector3Int.FloorToInt(position), Block.ConvertID(blockID));
        PlayerTerraformModule.Position.Remove(Vector3Int.FloorToInt(Machine.transform.position));
    }
}


public class BreakBlockInfo : DestructableInfo
{
    public override void Initialize()
    {
        Loot = Block.ConvertID(World.GetBlock(Vector3Int.FloorToInt(position)));
        Machine.transform.localScale = Vector3.one * 1.04f; 
        BlockPreview.Set(Machine.gameObject, "overlay");
    }
    
    public override void OnDestroy(Projectile projectile)
    { 
        base.OnDestroy(projectile);
        World.SetBlock(Vector3Int.FloorToInt(position));
        PlayerTerraformModule.Position.Remove(Vector3Int.FloorToInt(Machine.transform.position));
    }
}