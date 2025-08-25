using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerStatus {
    Active, 
    Dead,
    Loading,
}
[System.Serializable]
public class PlayerInfo : MobInfo
{ 
    public Storage Storage;
    public float Mana;
    public float Sanity;
    public int Hunger;
    public int HungerMax = 20;
    public float Stamina;   
    
    private const float JumpGraceTime = 0.1f; 
    private const float CoyoteTime = 0.1f; 
    [NonSerialized] public float AirTime;
    [NonSerialized] private float _jumpGraceTimer;
    [NonSerialized] private float _coyoteTimer;
    [NonSerialized] public PlayerStatus PlayerStatus = PlayerStatus.Loading;

    public override void Initialize()
    { 
        base.Initialize(); 
        IframesCurrent = 300;
        Storage.info = this;
        _ = new CoroutineTask(HungerClock());
    }

    protected override void OnHit(Projectile projectile)
    {
        GUIBar.Update(); 
        Machine.SetState<MobHit>();
    }

    private IEnumerator HungerClock()
    {
        while (!Destroyed)
        {
            if (Hunger <= 0)  
            {
                Health--;
                Audio.PlaySFX(SfxID.HitPlayer);
            }
            else Hunger--; 
            GUIBar.Update(); 
            yield return new WaitForSeconds(120);
        } 
    }
    
    protected override void OnUpdate()
    {
        base.OnUpdate();
        if (PlayerStatus == PlayerStatus.Active)
        {
            if (Health <= 0)
            {
                Audio.PlaySFX(SfxID.DeathPlayer);
                SpriteTool.gameObject.SetActive(false);
                CancelTarget();
                PlayerStatus = PlayerStatus.Dead;
                GUIMain.Show(false);
                Velocity = Vector2.zero; 
                IframesCurrent = 500;
                Machine.SetState<DeadState>();
            }

            FaceTarget = Equipment != null || Target != null;

            if (Game.PlayerInfo == this && (Target == null || ActionType != IActionType.PickUp && ActionType != IActionType.Interact))
            {
                TargetScreenDir = (Input.mousePosition - new Vector3(Screen.width / 2f, Screen.height / 2f, 0)).normalized;
                
                AimPosition = Control.MouseTarget ?
                    Control.MouseTarget.transform.position + Vector3.up * 0.55f :
                    Control.MousePosition + Vector3.up * 0.15f; 
                
                if (!IsInRenderRange) return;
                SpeedTarget = Control.Inst.Sprint.Key() ? SpeedAir : SpeedGround;
                HandleMovement();
            }
            else
            {
                if (Target != null) AimPosition = Target.position; 
                
                SpeedTarget = IsGrounded ? SpeedGround + 0.2f : SpeedAir * 2;
            } 
        }
        else
        { 
            if (IframesCurrent != 1) return; 
            if (PlayerStatus == PlayerStatus.Dead)
            {
                Machine.transform.position = WorldGen.SpawnPoint;
                SpriteTool.gameObject.SetActive(true);   
            } 
            Health = HealthMax;
            Hunger = HungerMax;
            Velocity = Vector2.zero;
            PlayerStatus = PlayerStatus.Active;
            GUIBar.Update();
            Inventory.RefreshInventory();
            Machine.SetState<DefaultState>();
        } 
        
        //fall damage
        if (!IsGrounded && Velocity.y < -10) AirTime += 1;
        else {
            if (AirTime > 75)
            {
                Health += (int)(Velocity.y * 3/ Gravity);
                GUIBar.Update();
                Audio.PlaySFX(SfxID.HitPlayer);
            }
            AirTime = 0;
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
            _coyoteTimer -= Helper.GetDeltaTime();; // Decrease coyote timer when not grounded
        }

        if (Control.Inst.Jump.KeyDown())
        {
            if (Game.Fly)
                Velocity.y = JumpVelocity;
            else 
                _jumpGraceTimer = JumpGraceTime; // Reset jump grace timer when jump key is pressed
        }
        else
        {
            _jumpGraceTimer -= Helper.GetDeltaTime();; // Decrease jump grace timer
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