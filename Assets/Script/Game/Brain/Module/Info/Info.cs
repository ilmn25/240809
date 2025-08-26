using System;
using System.Collections.Generic;
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
}