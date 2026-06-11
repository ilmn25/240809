using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class Info : EntityModule
{
    public static readonly Dictionary<string, Info> Dictionary = new Dictionary<string, Info>();
    public string uid = Guid.NewGuid().ToString("N");
    public ID id;
    public Vector3 position;
    /// <summary>Who owns this entity. 0 = host, -1 = free (host-owned until claimed), >0 = remote client connection ID.</summary>
    public int ownerId = 0;
    [NonSerialized] public bool Destroyed = false;
    [NonSerialized] public bool IsInRenderRange;
    public virtual bool OnHitInternal(Projectile projectile) { return false; }
    public virtual void AbstractHit(MobInfo info) { }
    public void Destroy() {Destroyed = true;}
    
    /// <summary>Does the local context (host or client) have authority over this entity?
    /// 0 = host, -1 = free (host-owned until claimed), >0 = remote client connection ID.</summary>
    public bool IsOwner()
    {
        if (Helper.IsHost())
            return ownerId == 0 || ownerId == -1;
        return ownerId == PlayerSync.MyConnectionId;
    }

    public override string ToString()
    {
        return $"Target: {Helper.ToDisplayName(id)}";
    }
}