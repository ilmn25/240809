public class StatusModule : HitboxModule
{
    private readonly HitboxType _hitBoxType;
    public float Health;
    private readonly float _defense;
    public float KnockBackResistance;
    
    private const int Iframes = 4;
    private int _iframesCurrent; 

    public StatusModule(HitboxType hitBoxType, float health, float defense)
    {
        _hitBoxType = hitBoxType;
        Health = health;
        _defense = defense;
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
                if (_hitBoxType == HitboxType.Enemy) return false;
                break;
            case HitboxType.Enemy: // player only kill enemy
                if (_hitBoxType == HitboxType.Friendly) return false;
                if (_hitBoxType == HitboxType.Passive) return false;
                break;
            case HitboxType.Passive: // friendly hit enemy and passive 
                if (_hitBoxType == HitboxType.Friendly) return false;
                break;
        }
        _iframesCurrent = Iframes;
        OnHit(projectile);
        Health -= projectile.Info.GetDamage() - _defense;
        return true;
    }
}