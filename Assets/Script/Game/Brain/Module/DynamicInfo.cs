using UnityEngine;

public class DynamicInfo : Info
{ 
    public HitboxType HitboxType;
    public string CharSprite;
    public string HurtSfx;
    public string DeathSfx; 
    public float KnockBackResistance = 1;
     
    public float SpeedGround = 5;
    public float SpeedAir = 10;
    public float AccelerationTime = 0.3f;
    public float DecelerationTime = 0.08f;
    public float Gravity = -40f;
    public float JumpVelocity = 10f; 
    
    public float Health; 
    public float HealthMax;
    public float Defense; 
    public float MapCollisionRadius = 0.3f;
    public float EntityCollisionRadius = 0.15f;
    
    public Transform Sprite;
    public Animator Animator;
    public Transform SpriteChar;
    public SpriteRenderer SpriteCharRenderer;
    public Transform SpriteToolTrack;
    public Transform SpriteTool;
    public SpriteRenderer SpriteToolRenderer; 
     
    protected int Iframes = 15;
    private int _iframesCurrent;  
    public float AirTime;
    public Vector3 Velocity = Vector3.zero;
    public bool IsGrounded = false;
    public Vector3 Direction = Vector3.zero; 
    public Vector3 TargetScreenDir; 

    private static readonly Collider[] ColliderArray = new Collider[3];
    
    protected virtual void OnHit(Projectile projectile) { }
    protected virtual void OnDeath() { } 
    protected virtual void OnUpdate() { 
        int hitCount = Physics.OverlapSphereNonAlloc(Machine.transform.position, EntityCollisionRadius, ColliderArray, Game.MaskEntity);
        for (int i = 0; i < hitCount; i++)
        {
            Collider col = ColliderArray[i];

            if (col.gameObject == Machine.gameObject)
                continue;

            KnockBack(col.transform.position, 0.4f, true);
            break; 
        }
    } 

    public override void Initialize()
    { 
        Sprite = Machine.transform.Find("sprite");
        Animator = Sprite.GetComponent<Animator>();
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

        if (!IsGrounded && Velocity.y < -10) AirTime += 1;
        else {
            if (AirTime > 75)
            {
                Health -= AirTime/8;
                Audio.PlaySFX(HurtSfx,0.4f);
            }
            AirTime = 0;
        }
        
        if (Health <= 0)
        { 
            Audio.PlaySFX(DeathSfx, 0.8f);
            Health = HealthMax;
            OnDeath();
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
        KnockBack(projectile.transform.position, projectile.Info.Knockback * KnockBackResistance, true);
        return true;
    }
    public void KnockBack(Vector3 position, float force, bool isAway)
    {
        position.y = Machine.transform.position.y;
        Velocity += (isAway? Machine.transform.position - position : Machine.transform.position + position).normalized * force;
    }
}