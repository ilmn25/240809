 
using UnityEngine;

public class PlayerMachine : Machine, IHitBox
{
    public override void OnInitialize()
    {
        AddModule(new PlayerMovementModule()); 
        AddModule(new PlayerAnimationModule()); 
        AddModule(new PlayerTerraformModule());
        AddModule(new PlayerStatusModule());
        AddState(new EquipState());  
        AddState(new PlayerState());  
    }

    public override void OnUpdate()
    { 
        GetModule<PlayerTerraformModule>().Update();
        GetModule<PlayerStatusModule>().Update();
    }

    public void OnHit(Projectile projectile)
    {
        if (projectile.Target == ProjectileTarget.Enemy || projectile.Target == ProjectileTarget.Passive) return;
        PlayerStatusModule.hit(projectile.Info.GetDamage(), projectile.Info.Knockback, projectile.transform.position);
    }
} 
