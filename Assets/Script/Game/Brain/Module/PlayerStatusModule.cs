using UnityEngine;

public class PlayerStatusModule : StatusModule
{ 
    public float Mana;
    public float Sanity;
    public float Hunger;
    public float Stamina;
    public float Speed;
    public float AirTime;
    public bool IsBusy = false;
    public bool Invincibility = false;
     
    public Vector3 TargetScreenDir;
    public Transform Sprite;
    public Transform SpriteChar;
    public SpriteRenderer SpriteCharRenderer;
    public Transform SpriteToolTrack;
    public Transform SpriteTool;
    public SpriteRenderer SpriteToolRenderer; 
    public static PlayerMovementModule _PlayerMovementModule;

    public PlayerStatusModule() : base(HitboxType.Friendly, PlayerData.Inst.health, 0)
    { 
        Mana = PlayerData.Inst.mana;
        Sanity = PlayerData.Inst.sanity;
        Hunger = PlayerData.Inst.hunger;
        Stamina = PlayerData.Inst.stamina;
        Speed = PlayerData.Inst.speed;
    }

    public override void Initialize()
    {
        Sprite = Machine.transform.Find("sprite");
        SpriteChar = Sprite.Find("char");
        SpriteCharRenderer = SpriteChar.GetComponent<SpriteRenderer>();
        SpriteToolTrack = Sprite.transform.Find("tool_track");
        SpriteTool = SpriteToolTrack.Find("tool");
        SpriteToolRenderer = SpriteTool.GetComponent<SpriteRenderer>();
        
        _PlayerMovementModule = Machine.GetModule<PlayerMovementModule>();
    }

    protected override void OnUpdate()
    {
        TargetScreenDir = (Input.mousePosition - new Vector3(Screen.width / 2f, Screen.height / 2f, 0)).normalized;
        
        if (!_PlayerMovementModule.IsGrounded && _PlayerMovementModule.Velocity.y < -10) AirTime += 1;
        else {
            if (AirTime > 75)
            {
                Health -= AirTime/8;
                Audio.PlaySFX("player_hurt",0.4f);
            }
            AirTime = 0;
        }
        
        if (Hunger > 0) Hunger -= 0.01f; 
    }

    protected override void OnDeath()
    {
        Audio.PlaySFX("player_die",0.5f);
        Health = PlayerData.Inst.health;
        Game.Player.transform.position = Utility.AddToVector(Game.Player.transform.position, 0,7, 0);
        Game.GameState = GameState.Loading;
    }

    protected override void OnHit(Projectile projectile)
    {
        _PlayerMovementModule.KnockBack(projectile.transform.position, projectile.Info.Knockback, true);
        Audio.PlaySFX("player_hurt",0.4f);
    }

    // for later passive effects boosts
    public static float GetRange()
    {
        return 1 * Inventory.CurrentItemData.Range;
    }
    public static float GetSpeed()
    {
        return 1 * Inventory.CurrentItemData.Speed;
    } 
}