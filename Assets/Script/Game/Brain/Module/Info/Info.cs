using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class Info : EntityModule
{
    public static readonly Dictionary<string, Info> Dictionary = new Dictionary<string, Info>();
    public ID id;
    public Vector3 position;
    [NonSerialized] public bool Destroyed = false;
    [NonSerialized] public bool IsInRenderRange; 
    public virtual bool OnHitInternal(Projectile projectile) { return false; }
    public virtual void AbstractHit(MobInfo info) { }
    public void Destroy() {Destroyed = true;}

    protected static string FormatId(ID value)
    {
        return Regex.Replace(value.ToString(), "(?<!^)([A-Z])", " $1");
    }

    public override string ToString()
    {
        return $"Target: {FormatId(id)}";
    }
}