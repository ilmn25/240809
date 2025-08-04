 
using UnityEngine;

public class PlayerMachine : Machine, IHitBox
{
    private PlayerStatusModule _playerStatusModule;
    private PlayerTerraformModule _playerTerraformModule;
    public override void OnInitialize()
    {
        AddModule(new PlayerMovementModule()); 
        AddModule(new PlayerAnimationModule()); 
        AddModule(new PlayerTerraformModule());
        AddModule(new PlayerStatusModule());
        AddState(new EquipState());  
        AddState(new PlayerState());
        _playerStatusModule = GetModule<PlayerStatusModule>();
        _playerTerraformModule = GetModule<PlayerTerraformModule>();
    }

    
    public override void OnUpdate()
    { 
        _playerStatusModule .Update();
        _playerTerraformModule.Update();
    }

    public void OnHit(Projectile projectile)
    {
        if (projectile.Target == ProjectileTarget.Enemy || projectile.Target == ProjectileTarget.Passive) return;
        _playerStatusModule.hit(projectile.Info.GetDamage(), projectile.Info.Knockback, projectile.transform.position);
    }
} 
