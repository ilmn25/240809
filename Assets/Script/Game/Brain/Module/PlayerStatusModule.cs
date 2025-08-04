using UnityEngine;

public class PlayerStatusModule : Module
{
    public float Health;
    public float Mana;
    public float Sanity;
    public float Hunger;
    public float Stamina;
    public float Speed;
    public float AirTime;
    public bool IsBusy = false;
    public bool Invincibility = false;
    public float CurrentIframes = 40;
    public float Iframes = 120;
    
    public static PlayerMovementModule _PlayerMovementModule;
    
    public override void Initialize()
    {
        _PlayerMovementModule = Machine.GetModule<PlayerMovementModule>();
        PlayerData.Load(); 
        Health = PlayerData.Inst.health;
        Mana = PlayerData.Inst.mana;
        Sanity = PlayerData.Inst.sanity;
        Hunger = PlayerData.Inst.hunger;
        Stamina = PlayerData.Inst.stamina;
        Speed = PlayerData.Inst.speed;
    }

    public void Update()
    {
        if (CurrentIframes > 0) CurrentIframes--;
        if (!_PlayerMovementModule.IsGrounded && _PlayerMovementModule._velocity.y < -10) AirTime += 1;
        else {
            if (AirTime > 75)
            {
                UpdateHealth(-AirTime/8);
                Audio.PlaySFX("player_hurt",0.4f);
            }
            AirTime = 0;
        }
        
        if (Hunger > 0) Hunger -= 0.01f;
        if (Health == 0)
        {
            Audio.PlaySFX("player_die",0.5f);
            Health = 100;
            Game.Player.transform.position = Utility.AddToVector(Game.Player.transform.position, 0,7, 0);
            Game.GameState = GameState.Loading;
        }
    }

    public void UpdateHealth(float amount)
    {
        Health += amount;
        if (Health > PlayerData.Inst.health) Health = PlayerData.Inst.health;
        else if (Health < 0) Health = 0;
    }

    public void hit(float dmg, float knockback, Vector3 position)
    {
        if (CurrentIframes != 0) return;
        CurrentIframes = Iframes;
        UpdateHealth(-dmg);
        _PlayerMovementModule.KnockBack(position, knockback, true);
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