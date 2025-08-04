using UnityEngine;

public class PlayerStatusModule : Module
{
    public static float Health;
    public static float Mana;
    public static float Sanity;
    public static float Hunger;
    public static float Stamina;
    public static float Speed;
    public static float AirTime;
    public static bool IsBusy = false;
    public static bool Invincibility = false;
    public static float CurrentIframes = 40;
    public static float Iframes = 120;
    
    public override void Initialize() 
    {
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
        if (!PlayerMovementModule.inst.IsGrounded && PlayerMovementModule.inst._velocity.y < -10) AirTime += 1;
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

    public static void UpdateHealth(float amount)
    {
        Health += amount;
        if (Health > PlayerData.Inst.health) Health = PlayerData.Inst.health;
        else if (Health < 0) Health = 0;
    }

    public static void hit(float dmg, float knockback, Vector3 position)
    {
        if (CurrentIframes != 0) return;
        CurrentIframes = Iframes;
        UpdateHealth(-dmg);
        PlayerMovementModule.inst.KnockBack(position, knockback, true);
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
    public static float GetKnockback()
    {
        return 1 * Inventory.CurrentItemData.ProjectileInfo.Knockback;
    }
}