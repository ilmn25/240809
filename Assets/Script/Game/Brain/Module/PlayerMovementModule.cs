using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
public class PlayerMovementModule : Module
{
    
    private PlayerStatusModule _playerStatusModule; 
    public Vector3 Velocity = Vector3.zero; 
    public bool IsGrounded = false;  
    public Vector2 RawInput;  
    private Vector2 _input = Vector2.zero;
    private readonly List<Vector2> _inputBuffer = new List<Vector2>();
    public float SpeedCurrent; 
    private float _speedTarget;
    private Vector3 _newPosition;  
    private readonly int _inputBufferCount = 4;
    private float _jumpGraceTimer;
    private float _coyoteTimer; 
    private float _deltaTime;  
    private float _speedAdjust;

    private float _orbitRotation;
    private float _cosAngle;
    private float _sinAngle;
    
    private const float SpeedWalk = 4f;
    private const float SpeedRun = 8f;
    private const float AccelerationTime = 0.2f; 
    private const float DecelerationTime = 0.08f; 
    private const float SlideDegree = 0.3f; 
    private const float Gravity = -40f;
    public const float JumpVelocity = 12f;
    private const float HoldVelocity = 0.05f;
    private const float ClampVelocity = 10f;
    private const float JumpGraceTime = 0.1f; 
    private const float CoyoteTime = 0.1f; 

    public void KnockBack(Vector3 position, float force, bool isAway)
    {
        Velocity += (isAway? Machine.transform.position - position : Machine.transform.position + position).normalized * force;
    }
    
    public void HandleInput()
    {
        RawInput = Control.GetMovementAxis();
        if (RawInput == Vector2.zero)
        {
            _input = Vector2.zero;
            return;
        }
        _orbitRotation = Mathf.Deg2Rad * -ViewPort.OrbitRotation;
        _cosAngle = Mathf.Cos(_orbitRotation);
        _sinAngle = Mathf.Sin(_orbitRotation);
        _input.x = RawInput.x * _cosAngle - RawInput.y * _sinAngle;
        _input.y = RawInput.x * _sinAngle + RawInput.y * _cosAngle; 
    }

    public override void Initialize()
    {
        _playerStatusModule = Machine.GetModule<PlayerStatusModule>(); 
    }

    public void HandleMovementUpdate()
    {  
        
        _deltaTime = Utility.GetDeltaTime();
        _newPosition = Machine.transform.position;

        // get input
        HandleInput();
        // handle jumping and GRAVITY
        HandleJump();

        if (_input != Vector2.zero)
        {
            // speeding up to start
            if (Control.Inst.Sprint.Key() && _playerStatusModule.Stamina > 1)
            {
                _speedTarget = SpeedRun;
                _playerStatusModule.Stamina -= 0.1f;
            }
            else
            {
                _speedTarget = SpeedWalk;
                _playerStatusModule.Stamina += 0.1f;
            }
            
            SpeedCurrent = Mathf.Lerp(SpeedCurrent, _speedTarget, _deltaTime / AccelerationTime);

            _speedAdjust = 1f; 
            if (RawInput.x != 0 && RawInput.y != 0)
            {
                _speedAdjust = 1 / Mathf.Sqrt(2); // This is equivalent to 1 / 1.41421
            }

            _newPosition.x += _input.x * SpeedCurrent * _deltaTime * _speedAdjust;
            _newPosition.z += _input.y * SpeedCurrent * _deltaTime * _speedAdjust;

            if (!IsMovable(_newPosition))
            {
                HandleObstacle(_newPosition);
            } 
 
            // Remove the first Vector2 from the list if it exceeds the desired size
            _inputBuffer.Add(_input); 
            if (_inputBuffer.Count > _inputBufferCount) _inputBuffer.RemoveAt(0);
        }
        else if (SpeedCurrent != 0)
        {
            // slowing down to stop
            SpeedCurrent = (SpeedCurrent < 0.05f) ? 0f : Mathf.Lerp(SpeedCurrent, 0, _deltaTime / DecelerationTime);

            _newPosition.x += _inputBuffer[0].x * SpeedCurrent * _deltaTime;
            _newPosition.z += _inputBuffer[0].y * SpeedCurrent * _deltaTime;

            if (!IsMovable(_newPosition))
            {
                SpeedCurrent /= 2;
                _newPosition = Machine.transform.position;
            }
        }

        HandleMove(); 
    }


    // private bool fly;

    private void HandleJump()
    {
        // if (Input.GetKeyDown(KeyCode.I)) fly = !fly;
        // if (fly && Control.Inst.Jump.KeyDown())
        // {
        //     _velocity.y = JumpVelocity;
        // }
        
        
        if ( IsGrounded)
        {
            _coyoteTimer = CoyoteTime; // Reset coyote timer when grounded
        }
        else
        {
            _coyoteTimer -= _deltaTime; // Decrease coyote timer when not grounded
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
            _jumpGraceTimer -= _deltaTime; // Decrease jump grace timer
        }

        if ((IsGrounded || _coyoteTimer > 0) && _jumpGraceTimer > 0)
        {
            Velocity.y = JumpVelocity;
            //  _isGrounded = false;
            _jumpGraceTimer = 0; // Reset jump grace timer after jumping
        }
        IsGrounded = Velocity.y == 0;
    }





















    float testPosition;
    Vector3 testPositionA;
    Vector3 testPositionB;
    private void HandleObstacle(Vector3 position)
    {
        _newPosition = Machine.transform.position;
        //! go any possible direction when going diagonally against a wall
        if (_input.x != 0 && IsMovable(new Vector3(position.x, Machine.transform.position.y, Machine.transform.position.z)))
        { 
            _newPosition = new Vector3(position.x, Machine.transform.position.y, Machine.transform.position.z);
        }
        else if (_input.y != 0 && IsMovable(new Vector3(Machine.transform.position.x, Machine.transform.position.y, position.z)))
        { 
            _newPosition = new Vector3(Machine.transform.position.x, Machine.transform.position.y, position.z);
        }
        else
        { 
            //! slide against wall if possible
            if (_input.x != 0)
            {
                testPosition = Machine.transform.position.x + SlideDegree * _input.x * SpeedCurrent * _deltaTime;
                testPositionA = new Vector3(testPosition, Machine.transform.position.y, Machine.transform.position.z);
                testPositionB = new Vector3(testPosition, Machine.transform.position.y, Machine.transform.position.z);
                testPositionA.z += -1 * SpeedCurrent * _deltaTime;
                testPositionB.z += 1 * SpeedCurrent * _deltaTime; 
            }
            else
            {
                testPosition = Machine.transform.position.z + SlideDegree * _input.y * SpeedCurrent * _deltaTime;
                testPositionA = new Vector3(Machine.transform.position.x, Machine.transform.position.y, testPosition);
                testPositionB = new Vector3(Machine.transform.position.x, Machine.transform.position.y, testPosition);
                testPositionA.x += -1 * SpeedCurrent * _deltaTime;
                testPositionB.x += 1 * SpeedCurrent * _deltaTime; 
            }
            
            if (IsMovable(testPositionA) && !IsMovable(testPositionB))
            {
                _newPosition = testPositionA;
            }
            else if (!IsMovable(testPositionA) && IsMovable(testPositionB))
            {
                _newPosition = testPositionB;
            }
        }
    }





















    Vector3 _tempPosition;
    private void HandleMove()
    { 
        if (Velocity.y > Gravity) //terminal velocity
        {
            Velocity.y += Gravity * _deltaTime;
        } 
        
        _tempPosition = new Vector3(_newPosition.x + Velocity.x * _deltaTime, _newPosition.y, _newPosition.z);
        if (!IsMovable(_tempPosition))
            Velocity.x = 0; 
        else
            _newPosition = _tempPosition;
        
        _tempPosition = new Vector3(_newPosition.x, _newPosition.y, _newPosition.z + Velocity.z * _deltaTime);
        if (!IsMovable(_tempPosition))
            Velocity.z = 0; 
        else
            _newPosition = _tempPosition;
        
        Velocity.x = Mathf.MoveTowards(Velocity.x, 0f, 30 * Time.deltaTime);
        Velocity.z = Mathf.MoveTowards(Velocity.z, 0f, 30 * Time.deltaTime);
        
        _tempPosition = new Vector3(_newPosition.x, _newPosition.y + Velocity.y * _deltaTime, _newPosition.z);
        if (!IsMovable(_tempPosition))
        {
            if (Velocity.y < 0) IsGrounded = true; 
            Velocity.y = 0; 
            Machine.transform.position = _newPosition;
        } 
        else 
        {
            Machine.transform.position = _tempPosition;
        }
    }

    private readonly Collider[] _tempCollisionArray = new Collider[1];
    private readonly Vector3 _colliderSize = new Vector3(0.35f, 0.35f, 0.25f);  
    private readonly Vector3 _colliderCenter = new Vector3(0, 0.35f, 0); 
    private bool IsMovable(Vector3 position)
    { 
        return !(Physics.OverlapBoxNonAlloc(position + _colliderCenter, 
            _colliderSize, _tempCollisionArray, Quaternion.identity, 
            Game.MaskStatic) > 0);
    }
 
}
