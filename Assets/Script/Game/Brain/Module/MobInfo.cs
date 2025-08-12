using UnityEngine;

public enum IActionTarget {Primary, Secondary, Hit}
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
    public int PathAir = 4;
    public int PathAmount = 3000;
    public int MaxStuckCount = 250;    
    
    public Item Equipment;
 
    public Transform Target;
    public IAction Action;
    public IActionTarget ActionTarget;
    public bool FaceTarget;
    public Vector3 AimPosition;
    public HitboxType TargetHitboxType;
    public PathingStatus PathingStatus = PathingStatus.Pending; 

    protected override void OnUpdate()
    { 
        base.OnUpdate();
        if (Target)
        {
            TargetScreenDir = (Camera.main.WorldToScreenPoint(Target.transform.position) - 
                              Camera.main.WorldToScreenPoint(Machine.transform.position)).normalized;
        }
    }    
    
    public void SetEquipment(Item item)
    { 
        if (item != null)
        {
            Equipment = item;
            SpriteTool.gameObject.SetActive(true);
            SpriteTool.localPosition = new Vector3(item.HoldoutOffset.x, item.HoldoutOffset.y, 0);
            SpriteToolRenderer.sprite = Cache.LoadSprite("sprite/" + item.StringID);
            SpriteToolTrack.transform.localScale = Vector3.one * item.Scale;
            Machine.SetState<EquipSelectState>();
        }
        else
        {
            Equipment = null;
            SpriteTool.gameObject.SetActive(false);
        }
    }
}

public class EnemyInfo : MobInfo
{
    public override void Initialize()
    {
        base.Initialize();
        Health = HealthMax;
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        FaceTarget = Target;
        SpeedTarget = IsGrounded? SpeedGround : SpeedAir; 
        if (Health <= 0)
        { 
            Loot.Gettable(((EntityMachine)Machine).entityData.stringID).Spawn(Machine.transform.position);
            ((EntityMachine)Machine).Delete();
            Audio.PlaySFX(DeathSfx, 0.8f);
        }
    }

    protected override void OnHit(Projectile projectile)
    { 
        Target = projectile.Source.transform;
        PathingStatus = PathingStatus.Reached; 
    }
 
}