using UnityEngine;

public class MobStatusModule : StatusModule
{ 
    public float AttackDistance;
    public Item Equipment; 
     
    public Transform Target = null;
    public Vector3 TargetScreenDir; 
    public PathingStatus PathingStatus = PathingStatus.Pathing;
    public Vector3 Direction = Vector3.zero; 
    
    protected override void OnUpdate()
    {
        if (Target)
        {
            TargetScreenDir = (Camera.main.WorldToScreenPoint(Target.transform.position) - 
                              Camera.main.WorldToScreenPoint(Machine.transform.position)).normalized;
        } 
    }

    protected override void OnHit(Projectile projectile)
    { 
        Target = Game.Player.transform;
        PathingStatus = PathingStatus.Reached; 
    }

    protected override void OnDeath()
    { 
        Loot.Gettable(((EntityMachine)Machine).entityData.stringID).Spawn(Machine.transform.position);
        ((EntityMachine)Machine).Delete();
    }
}