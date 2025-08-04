public class RangedProjectileInfo : ProjectileInfo
{ 
    public float LifeSpan;
    public float Speed;
    public float Frequency;
    public float Penetration;
    public bool Friendly = true;
    
    public override void AI(Projectile projectile)
    {
        if (projectile.LifeSpan > LifeSpan) projectile.Delete();
    }
}