using System;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Info : EntityModule
{
    public static readonly Dictionary<string, Info> Dictionary = new Dictionary<string, Info>();
    public string stringID;
    public Vector3 position;
    [NonSerialized] public bool Destroyed = false;
    public SetEntity ToSetPieceInfo()
    {
        return new SetEntity()
        {
            stringID = stringID,
            position = Vector3Int.FloorToInt(position)
        };
    }
    public virtual bool OnHitInternal(Projectile projectile) { return false; }
}