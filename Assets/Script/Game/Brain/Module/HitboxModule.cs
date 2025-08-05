using UnityEngine;

public class HitboxModule : Module 
{
    public virtual bool OnHitInternal(Projectile projectile) { return false; }
}