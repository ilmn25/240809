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
    /// <summary>Who owns this entity (runs AI/pathfinding). 0 = host (free, host-managed), >0 = remote client.</summary>
    public int ownerId = 0;
    /// <summary>Who controls this player (provides input). -1 = free, 0 = host, >0 = remote client.</summary>
    public int controllerId = -1;
    /// <summary>Whether this entity is flammable (can catch and spread fire).</summary>
    public bool Flammable;
    /// <summary>Current fire intensity, 0 = not burning, 1 = fully burning. Persisted for save/load.</summary>
    public float FireLevel;
    [NonSerialized] public bool Destroyed = false;
    [NonSerialized] public bool IsInRenderRange;
    public virtual bool OnHitInternal(Projectile projectile) { return false; }
    public virtual void AbstractHit(Projectile projectile) { }
    public void Destroy() {Destroyed = true;}
    
    /// <summary>Does the local context (host or client) have authority over this entity?
    /// 0 = host, >0 = remote client connection ID.</summary>
    public bool IsOwner()
    {
        if (Helper.IsHost())
            return ownerId == 0 || controllerId == 0;
        return ownerId == PlayerSync.MyConnectionId || controllerId == PlayerSync.MyConnectionId;
    }

    public override string ToString()
    {
        return $"Target: {Helper.ToDisplayName(id)}";
    }
}