 
using Unity.VisualScripting;
using UnityEngine; 

public class HunterMachine : EntityMachine, IHitBox
{ 
    public override void OnInitialize()
    {
        Transform spriteTransform = transform.Find("sprite").transform;
        AddModule(new MobStatusModule(HitboxType.Enemy,100,1, "npc_hurt", "player_death"));
        AddModule(new GroundMovementModule());
        AddModule(new GroundPathingModule());
        AddModule(new GroundAnimationModule()); 
        AddModule(new MobSpriteCullModule()); 
        AddModule(new MobSpriteOrbitModule()); 
        AddState(new MobGroundState());  
        GetState<MobGroundState>().AddState(new MobAttackShoot());
    }

    public void OnDrawGizmos()
    {
        GetModule<GroundPathingModule>().DrawGizmos();
    }
}