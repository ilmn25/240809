using UnityEngine;

public enum PlayerStatus {
    Active, 
    Dead
}
public class PlayerInfo : MobInfo
{ 
    public float Mana;
    public float Sanity;
    public float Hunger;
    public float Stamina; 
    public bool IsBusy = false; 
    public PlayerStatus PlayerStatus = PlayerStatus.Dead;
    
    private const float JumpGraceTime = 0.1f; 
    private const float CoyoteTime = 0.1f; 
    private const float HoldVelocity = 0.05f;
    private const float ClampVelocity = 10f;
    private float _jumpGraceTimer;
    private float _coyoteTimer; 
    public override void Initialize()
    { 
        base.Initialize();
        IframesCurrent = 400;
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        if (PlayerStatus == PlayerStatus.Dead && Iframes == 1)
        {
            Machine.transform.position = new Vector3(
                World.ChunkSize * WorldGen.Size.x / 2,
                World.ChunkSize * WorldGen.Size.y - 5,
                World.ChunkSize * WorldGen.Size.z / 2);
            Sprite.gameObject.SetActive(true);
            Health = HealthMax;
        }
        FaceTarget = Equipment != null;
        TargetScreenDir = (Input.mousePosition - new Vector3(Screen.width / 2f, Screen.height / 2f, 0)).normalized;
        if (Hunger > 0) Hunger -= 0.01f;
        HandleMovement();
    }

    private void HandleMovement()
    {
        SpeedTarget = Control.Inst.Sprint.Key() && Stamina > 1 ? SpeedTarget : SpeedAir;
        
        if (IsGrounded)
        {
            _coyoteTimer = CoyoteTime; // Reset coyote timer when grounded
        }
        else
        {
            _coyoteTimer -= Utility.GetDeltaTime();; // Decrease coyote timer when not grounded
        }

        if (Control.Inst.Jump.KeyDown())
        {
            _jumpGraceTimer = JumpGraceTime; // Reset jump grace timer when jump key is pressed
        }
        else if (Control.Inst.Jump.KeyDown() && Velocity.y > ClampVelocity)
        {
            Velocity.y += HoldVelocity;
        }
        else
        {
            _jumpGraceTimer -= Utility.GetDeltaTime();; // Decrease jump grace timer
        }

        if ((IsGrounded || _coyoteTimer > 0) && _jumpGraceTimer > 0)
        {
            Velocity.y = JumpVelocity;
            //  _isGrounded = false;
            _jumpGraceTimer = 0; // Reset jump grace timer after jumping
        }
        IsGrounded = Velocity.y == 0;
        
        
        
        Vector2 rawInput = Control.GetMovementAxis();
        if (rawInput == Vector2.zero)
        {
            Direction = Vector2.zero;
            return;
        }
        float orbitRotation = Mathf.Deg2Rad * -ViewPort.OrbitRotation;
        float cosAngle = Mathf.Cos(orbitRotation);
        float sinAngle = Mathf.Sin(orbitRotation);
        Direction.x = rawInput.x * cosAngle - rawInput.y * sinAngle;
        Direction.z = rawInput.x * sinAngle + rawInput.y * cosAngle; 
    }

    protected override void OnDeath()
    { 
        Game.Player.transform.position = Utility.AddToVector(Game.Player.transform.position, 0,7, 0);
        Sprite.gameObject.SetActive(false);
        PlayerStatus = PlayerStatus.Dead;
        GUIMain.Show(false);
        IframesCurrent = 1000;
    }
 
    // for later passive effects boosts
    public float GetRange()
    {
        return 1 * Inventory.CurrentItemData.Range;
    }
    public float GetSpeed()
    {
        return 1 * Inventory.CurrentItemData.Speed;
    } 
}