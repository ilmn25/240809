public class StatusModule : HitboxModule
{
    public HitboxType HitBoxType;
    public float Health;
    public float Defense;
    private int _iframesCurrent;
    private const int Iframes = 4;

    public StatusModule(HitboxType hitBoxType, float health, float defense)
    {
        HitBoxType = hitBoxType;
        Health = health;
        Defense = defense;
    }

    protected virtual void OnHit(Projectile projectile) { }
    protected virtual void OnDeath() { } 
    protected virtual void OnUpdate() { } 

    public override void Update()
    { 
        OnUpdate();
        if (_iframesCurrent > 0) _iframesCurrent--;

        if (Health <= 0)
        {
            OnDeath();
            Health = 100;
        }
    }
    
    public override bool OnHitInternal(Projectile projectile)
    {
        if (_iframesCurrent != 0) return false;
        switch (projectile.Target)
        {
            case HitboxType.Friendly: // enemy kill friendly 
                if (HitBoxType == HitboxType.Enemy) return false;
                break;
            case HitboxType.Enemy: // player only kill enemy
                if (HitBoxType == HitboxType.Friendly) return false;
                if (HitBoxType == HitboxType.Passive) return false;
                break;
            case HitboxType.Passive: // friendly hit enemy and passive 
                if (HitBoxType == HitboxType.Friendly) return false;
                break;
        }
        _iframesCurrent = Iframes;
        OnHit(projectile);
        Health -= projectile.Info.GetDamage() - Defense;
        return true;
    }
}