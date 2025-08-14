using UnityEngine;

public enum PlayerStatus {
    Active, 
    Dead,
    Loading,
}
public class PlayerInfo : MobInfo
{ 
    public float Mana;
    public float Sanity;
    public float Hunger;
    public float Stamina; 
    public bool IsBusy = false; 
    public PlayerStatus PlayerStatus = PlayerStatus.Loading;
    
    private const float JumpGraceTime = 0.1f; 
    private const float CoyoteTime = 0.1f; 
    private const float HoldVelocity = 0.05f;
    private const float ClampVelocity = 10f;
    private float _jumpGraceTimer;
    private float _coyoteTimer; 
    public override void Initialize()
    { 
        base.Initialize();
        IframesCurrent = 300;
        IsPlayer = true;    
    }

    protected override void OnHit(Projectile projectile)
    {
        GUIHealthBar.Update();
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        
        if (PlayerStatus == PlayerStatus.Active)
        { 
            if (Health <= 0)
            { 
                Audio.PlaySFX(DeathSfx, 0.8f);
                Sprite.gameObject.SetActive(false);
                PlayerStatus = PlayerStatus.Dead;
                GUIMain.Show(false); 
                Velocity = Vector2.zero; 
                Direction = Vector2.zero;
                IframesCurrent = 500;
            }
            
            FaceTarget = Equipment != null || Target;  
            
            if (Hunger > 0) Hunger -= 0.01f; 
            if (!Target)
            {
                TargetScreenDir = (Input.mousePosition - new Vector3(Screen.width / 2f, Screen.height / 2f, 0)).normalized;
                SpeedTarget = Control.Inst.Sprint.Key() ? SpeedAir : SpeedGround;
                HandleMovement();
            }
            else
            {
                SpeedTarget = IsGrounded ? SpeedGround : SpeedAir * 1.5f;
            } 
        }
        else
        { 
            if (IframesCurrent != 1) return; 
            if (PlayerStatus == PlayerStatus.Dead)
            {
                Machine.transform.position = new Vector3(
                    World.ChunkSize * WorldGen.Size.x / 2,
                    World.ChunkSize * WorldGen.Size.y - 5,
                    World.ChunkSize * WorldGen.Size.z / 2);
                Sprite.gameObject.SetActive(true);  
            } 
            Health = HealthMax;
            PlayerStatus = PlayerStatus.Active;
            GUIHealthBar.Update();
            Inventory.RefreshInventory();
        } 
    }

    private void HandleMovement()
    { 
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
        Direction = Direction.normalized; 
        Direction = new Vector3(
            Direction.x > 0.001f ? 1f : Direction.x < -0.001f ? -1f : 0f,
            0f,
            Direction.z > 0.001f ? 1f : Direction.z < -0.001f ? -1f : 0f
        );
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