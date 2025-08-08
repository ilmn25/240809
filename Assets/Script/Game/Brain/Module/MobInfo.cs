using UnityEngine;

public class MobInfo : DynamicInfo
{ 
    public int DistAttack = 2;
    public int DistAlert = 10;
    public int DistDisengage = 20; 
    
    public int DistEscape = 20;
    public int DistRoam = 10;
    public int DistStrafe = 2;
    
    public int PathJump = 1;
    public int PathHeight = 1;
    public int PathFall = 15;
    public int PathAir = 3;
    public int PathAmount = 500;
    
    public Item Equipment;
     
    public Transform Target; 
    public PathingStatus PathingStatus = PathingStatus.Pending;

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