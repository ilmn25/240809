using System;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class DynamicInfo : Info
{
    private const int KnockbackInterval = 3;
    public HitboxType HitboxType;
    public ID CharSprite = ID.Null;
    public SfxID HitSfx = SfxID.HitMob;
    public SfxID DeathSfx = SfxID.DeathPlayer; 
    public float KnockBackResistance = 1;
     
    public float SpeedLogic = 5;
    public float SpeedGround = 5;
    public float SpeedAir = 10;  
    public float Gravity = -40f;
    public float JumpVelocity = 10f; 
    
    public int Health; 
    public int HealthMax;
    public int Defense; 
    public int Iframes = 5;
    
    [NonSerialized] public readonly float EntityCollisionRadius = 0.25f;
    [NonSerialized] public float AccelerationTime = 0.3f;
    [NonSerialized] public float DecelerationTime = 0.08f;
    [NonSerialized] public int KnockbackCounter = 0;
    
    [NonSerialized] public Transform Sprite;
    [NonSerialized] public Animator Animator;
    [NonSerialized] public Transform SpriteChar;
    [NonSerialized] public SpriteRenderer SpriteCharRenderer;
    [NonSerialized] public Transform SpriteToolTrack;
    [NonSerialized] public Transform SpriteTool;
    [NonSerialized] public SpriteRenderer SpriteToolRenderer;
 
    [NonSerialized] protected int IframesCurrent;
 
    [NonSerialized] public Vector3 Velocity = Vector3.zero;
    [NonSerialized] public bool IsGrounded = false;
    [NonSerialized] public Vector3 Direction = Vector3.zero;
    [NonSerialized] public Vector3 TargetPointPosition; 
    [NonSerialized] public Vector3 TargetScreenDir;
    [NonSerialized] public float SpeedCurrent;
    [NonSerialized] public float SpeedTarget = 10;

    private static readonly Collider[] ColliderArray = new Collider[3];
    
    protected virtual void OnHit(Projectile projectile) { } 
    protected virtual void OnUpdate() {
        if (Machine)
        {
            position = Machine.transform.position;
            IsInRenderRange = SpriteCharRenderer.isVisible && MapLoad.ActiveChunks.ContainsKey(World.GetChunkCoordinate(Machine.transform.position));
        }
        

        if (KnockbackCounter != KnockbackInterval)
        {
            KnockbackCounter++;
            return;
        }
        KnockbackCounter = 0;
        
        int hitCount = Physics.OverlapSphereNonAlloc(Machine.transform.position, EntityCollisionRadius, ColliderArray, Game.MaskEntity);
        for (int i = 0; i < hitCount; i++)
        {
            Collider col = ColliderArray[i];

            if (col.gameObject == Machine.gameObject || col.gameObject.layer != Game.IndexSemiCollide)
                continue;
            
            if (Game.Player && col.gameObject == Game.Player)
                KnockBack(col.transform.position, 1, true);
            else
                KnockBack(col.transform.position, 2 , true);
            break; 
        }
    } 

    public override void Initialize()
    { 
        Sprite = Machine.transform.Find("Sprite");
        Animator = Sprite.GetComponent<Animator>();
        SpriteChar = Sprite.Find("Char");
        SpriteCharRenderer = SpriteChar.GetComponent<SpriteRenderer>();
        SpriteToolTrack = Sprite.transform.Find("ToolTrack");
        SpriteTool = SpriteToolTrack.Find("Tool").Find("ToolOffset");
        SpriteToolRenderer = SpriteTool.GetComponent<SpriteRenderer>(); 
    }
    public override void Update()
    { 
        OnUpdate();
        if (IframesCurrent > 0) IframesCurrent--; 
    }   
    
    public override bool OnHitInternal(Projectile projectile)
    { 
        if (IframesCurrent != 0) return false;
        switch (projectile.TargetHitBoxType)
        {
            case HitboxType.Player:  
                if (this != Game.PlayerInfo) return false;
                break;
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
        IframesCurrent = Iframes; 
        Audio.PlaySFX(HitSfx);
        Health -= projectile.Info.GetDamage() - Defense;
        KnockBack(projectile.transform.position, projectile.Info.Knockback * KnockBackResistance, true);
        OnHit(projectile);
        return true;
    }
    public void KnockBack(Vector3 position, float force, bool isAway)
    {
        position.y = Machine.transform.position.y;
        Velocity += (isAway? Machine.transform.position - position : Machine.transform.position + position).normalized * force;
    }
}