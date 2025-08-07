using UnityEngine;

public class StatusModule : HitboxModule
{ 
    
    public HitboxType HitboxType;
    public string HurtSfx;
    public string DeathSfx; 
    public float KnockBackResistance;
    
    public float Defense; 
    public float Health;
    public float HealthMax;
    
    public Transform Sprite;
    public Transform SpriteChar;
    public SpriteRenderer SpriteCharRenderer;
    public Transform SpriteToolTrack;
    public Transform SpriteTool;
    public SpriteRenderer SpriteToolRenderer; 
    
    private const int Iframes = 4;
    private int _iframesCurrent;  
    public bool IsGrounded = false;

    protected virtual void OnHit(Projectile projectile) { }
    protected virtual void OnDeath() { } 
    protected virtual void OnUpdate() { } 

    public override void Initialize()
    { 
        Sprite = Machine.transform.Find("sprite");
        SpriteChar = Sprite.Find("char");
        SpriteCharRenderer = SpriteChar.GetComponent<SpriteRenderer>();
        SpriteToolTrack = Sprite.transform.Find("tool_track");
        SpriteTool = SpriteToolTrack.Find("tool");
        SpriteToolRenderer = SpriteTool.GetComponent<SpriteRenderer>();
        
        Health = HealthMax;
    }
    public override void Update()
    { 
        OnUpdate();
        if (_iframesCurrent > 0) _iframesCurrent--;

        if (Health <= 0)
        {
            OnDeath();
            Audio.PlaySFX(DeathSfx, 0.8f);
            Health = HealthMax;
        }
    }
    
    public override bool OnHitInternal(Projectile projectile)
    {
        if (_iframesCurrent != 0) return false;
        switch (projectile.Target)
        {
            case HitboxType.Friendly: // enemy kill friendly 
                if (HitboxType == HitboxType.Enemy) return false;
                break;
            case HitboxType.Enemy: // player only kill enemy
                if (HitboxType == HitboxType.Friendly) return false;
                if (HitboxType == HitboxType.Passive) return false;
                break;
            case HitboxType.Passive: // friendly hit enemy and passive 
                if (HitboxType == HitboxType.Friendly) return false;
                break;
        }
        _iframesCurrent = Iframes;
        OnHit(projectile);
        Audio.PlaySFX(HurtSfx, 0.4f);
        Health -= projectile.Info.GetDamage() - Defense;
        Machine.GetModule<GroundMovementModule>().KnockBack(projectile.transform.position, 
            projectile.Info.Knockback * KnockBackResistance, true);
        return true;
    }
}