 
public class BugMachine : EntityMachine, IHitBox
{ 
    public override void OnInitialize()
    {    
        AddModule(new MobStatusModule
        {
            HitboxType = HitboxType.Enemy,
            HealthMax = 100,
            Defense = 1,
            AttackDistance = 3,
            HurtSfx = "npc_hurt", 
            DeathSfx = "player_die"
        });
        AddModule(new GroundMovementModule(jumpVelocity: 15f));
        AddModule(new GroundPathingModule(1, 2, 15, 6));
        AddModule(new GroundAnimationModule()); 
        AddModule(new MobSpriteCullModule()); 
        AddModule(new MobSpriteOrbitModule()); 
        AddState(new MobGroundState()); 
        GetState<MobGroundState>().AddState(new MobAttackPounce());
    }

    public void OnDrawGizmos()
    {
        GetModule<GroundPathingModule>().DrawGizmos();
    }
} 