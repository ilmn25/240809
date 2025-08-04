using UnityEngine;

public class StatusModule : Module
{
    public HitboxType HitBoxType;
    public float Health;
    public float Defense;
    private int _iframesCurrent;
    private const int Iframes = 50;

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
    
    public void OnHitInternal(Projectile projectile)
    {
        if (_iframesCurrent != 0) return;
        switch (projectile.Target)
        {
            case HitboxType.Friendly: // enemy kill friendly 
                if (HitBoxType == HitboxType.Enemy) return;
                break;
            case HitboxType.Enemy: // player only kill enemy
                if (HitBoxType == HitboxType.Friendly) return;
                if (HitBoxType == HitboxType.Passive) return;
                break;
            case HitboxType.Passive: // friendly hit enemy and passive 
                if (HitBoxType == HitboxType.Friendly) return;
                break;
        }
        _iframesCurrent = Iframes;
        OnHit(projectile);
        Health -= projectile.Info.GetDamage() - Defense;
    }
     
}
public class PlayerStatusModule : StatusModule
{ 
    public float Mana;
    public float Sanity;
    public float Hunger;
    public float Stamina;
    public float Speed;
    public float AirTime;
    public bool IsBusy = false;
    public bool Invincibility = false;
    
    public static PlayerMovementModule _PlayerMovementModule;

    public PlayerStatusModule(HitboxType hitBoxType) : base(hitBoxType, PlayerData.Inst.health, 0)
    { 
        Mana = PlayerData.Inst.mana;
        Sanity = PlayerData.Inst.sanity;
        Hunger = PlayerData.Inst.hunger;
        Stamina = PlayerData.Inst.stamina;
        Speed = PlayerData.Inst.speed;
    }

    public override void Initialize()
    {
        _PlayerMovementModule = Machine.GetModule<PlayerMovementModule>();
    }

    protected override void OnUpdate()
    {
        if (!_PlayerMovementModule.IsGrounded && _PlayerMovementModule._velocity.y < -10) AirTime += 1;
        else {
            if (AirTime > 75)
            {
                Health -= AirTime/8;
                Audio.PlaySFX("player_hurt",0.4f);
            }
            AirTime = 0;
        }
        
        if (Hunger > 0) Hunger -= 0.01f; 
    }

    protected override void OnDeath()
    {
        Audio.PlaySFX("player_die",0.5f);
        Health = PlayerData.Inst.health;
        Game.Player.transform.position = Utility.AddToVector(Game.Player.transform.position, 0,7, 0);
        Game.GameState = GameState.Loading;
    }

    protected override void OnHit(Projectile projectile)
    {
        _PlayerMovementModule.KnockBack(projectile.transform.position, projectile.Info.Knockback, true);
        Audio.PlaySFX("player_hurt",0.4f);
    }

    // for later passive effects boosts
    public static float GetRange()
    {
        return 1 * Inventory.CurrentItemData.Range;
    }
    public static float GetSpeed()
    {
        return 1 * Inventory.CurrentItemData.Speed;
    } 
}