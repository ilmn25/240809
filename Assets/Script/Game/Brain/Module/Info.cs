using System.Collections.Generic;
using UnityEngine;

public class Info : Module
{
    public static readonly Dictionary<string, Info> Dictionary = new Dictionary<string, Info>();
    
    public virtual bool OnHitInternal(Projectile projectile) { return false; }
}