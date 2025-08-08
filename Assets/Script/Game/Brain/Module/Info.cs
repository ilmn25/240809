using UnityEngine;

public class Info : Module 
{
    public virtual bool OnHitInternal(Projectile projectile) { return false; }
}