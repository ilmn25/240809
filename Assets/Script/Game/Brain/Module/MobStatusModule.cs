using UnityEngine;

public class MobStatusModule : StatusModule
{
    public Transform Sprite;
    public Transform SpriteChar;
    public SpriteRenderer SpriteCharRenderer;
    public Transform SpriteToolTrack;
    public Transform SpriteTool;
    public SpriteRenderer SpriteToolRenderer; 
    
    public Transform Target = null;
    public Vector3 TargetScreenDir;
    public Item Equipment;
    
    public PathingStatus PathingStatus = PathingStatus.Pathing;
    public Vector3 Direction = Vector3.zero;
    public bool IsGrounded = false;

    private readonly string _hurtSfx;
    private readonly string _deathSfx;
     
    public override void Initialize()
    {
        Sprite = Machine.transform.Find("sprite");
        SpriteChar = Sprite.Find("char");
        SpriteCharRenderer = SpriteChar.GetComponent<SpriteRenderer>();
        SpriteToolTrack = Sprite.transform.Find("tool_track");
        SpriteTool = SpriteToolTrack.Find("tool");
        SpriteToolRenderer = SpriteTool.GetComponent<SpriteRenderer>();
    }
    
    protected override void OnUpdate()
    {
        if (Target)
        {
            TargetScreenDir = (Camera.main.WorldToScreenPoint(Target.transform.position) - 
                              Camera.main.WorldToScreenPoint(Machine.transform.position)).normalized;
        } 
    }

    public MobStatusModule(HitboxType hitBoxType, float health, float defense, string hurtSfx, string deathSfx) : base(
        hitBoxType, health, defense)
    {
        _hurtSfx = hurtSfx;
        _deathSfx = deathSfx;
    }

    protected override void OnHit(Projectile projectile)
    {
        Audio.PlaySFX(_hurtSfx, 0.4f);
        Target = Game.Player.transform;
        PathingStatus = PathingStatus.Reached;
        Machine.GetModule<GroundMovementModule>().KnockBack(projectile.transform.position, projectile.Info.Knockback, true);
    }

    protected override void OnDeath()
    {
        Audio.PlaySFX(_deathSfx, 0.8f);
        Loot.Gettable(((EntityMachine)Machine).entityData.stringID).Spawn(Machine.transform.position);
        ((EntityMachine)Machine).Delete();
    }
}