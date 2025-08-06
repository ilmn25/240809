 
public class BugMachine : EntityMachine, IHitBox
{ 
    public override void OnInitialize()
    {   
        AddState(new GhoulState()); 
        AddModule(new MobStatusModule(HitboxType.Enemy,60,1, "npc_hurt", "player_die"));
        AddModule(new GroundMovementModule(3, jumpVelocity: 15f));
        AddModule(new GroundPathingModule(1, 2, 15, 6));
        AddModule(new GroundAnimationModule()); 
        AddModule(new SpriteCullModule()); 
        AddModule(new SpriteOrbitModule()); 
    }

    public void OnDrawGizmos()
    {
        GetModule<GroundPathingModule>().DrawGizmos();
    }
} 