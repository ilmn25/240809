 
using UnityEngine;

public class PlayerMachine : Machine, IHitBox
{
    public override void OnInitialize()
    { 
        PlayerData.Load(); 
        AddModule(new PlayerStatusModule()
        {
            HitboxType = HitboxType.Friendly,
            HealthMax = PlayerData.Inst.health,
            Defense = 0,
            Mana = PlayerData.Inst.mana,
            Sanity = PlayerData.Inst.sanity,
            Hunger = PlayerData.Inst.hunger,
            Stamina = PlayerData.Inst.stamina,
            Speed = PlayerData.Inst.speed,
            DeathSfx = "player_die",
            HurtSfx = "player_hurt"
        });
        AddModule(new PlayerMovementModule()); 
        AddModule(new PlayerAnimationModule()); 
        AddModule(new PlayerTerraformModule()); 
        AddState(new PlayerState());  
    }
} 
