using System;
using UnityEngine;

[System.Serializable]
public class BlockInfo : DestructableInfo
{
    public string texture;
    public int blockID; 

    public override void Initialize()
    {
        if (operationType == OperationType.Dig)
        { 
            Loot = Block.ConvertID(World.GetBlock(Vector3Int.FloorToInt(position)));
            Machine.transform.localScale = Vector3.one * 1.04f; 
        } 
        else
            Machine.transform.localScale = Vector3.one;
        
        BlockPreview.Set(Machine.gameObject, texture);
    }
    
    public override void OnDestroy(Projectile projectile)
    { 
        base.OnDestroy(projectile);
        World.SetBlock(Vector3Int.FloorToInt(position), blockID);
    }
}