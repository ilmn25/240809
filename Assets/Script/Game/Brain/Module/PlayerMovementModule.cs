using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.AI;
public class PlayerMovementModule : Module
{
    private bool _fly = false;
    public float VerticalVelocity = 0f; 
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
    
    private const float SpeedWalk = 8f;
    private const float SpeedRun = 25f;
    private const float AccelerationTime = 0.2f; 
    private const float DecelerationTime = 0.08f; 
    private const float SlideDegree = 0.3f; 
    private const float Gravity = -40f;
    public const float JumpVelocity = 12f;
    private const float HoldVelocity = 0.05f;
    private const float ClampVelocity = 10f;
    private const float JumpGraceTime = 0.1f; 
    private const float CoyoteTime = 0.1f; 

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
            if (Control.Inst.Sprint.Key() && PlayerStatus.Stamina > 1)
            {
                _speedTarget = SpeedRun;
                PlayerStatus.Stamina -= 0.1f;
            }
            else
            {
                _speedTarget = SpeedWalk;
                PlayerStatus.Stamina += 0.1f;
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


    private bool fly;

    private void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.I)) fly = !fly;
        if (fly && Control.Inst.Jump.KeyDown())
        {
            VerticalVelocity = JumpVelocity;
        }
        
        
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
        else if (Control.Inst.Jump.KeyDown() && VerticalVelocity > ClampVelocity)
        {
            VerticalVelocity += HoldVelocity;
        }
        else
        {
            _jumpGraceTimer -= _deltaTime; // Decrease jump grace timer
        }

        if ((IsGrounded || _coyoteTimer > 0) && _jumpGraceTimer > 0)
        {
            VerticalVelocity = JumpVelocity;
            //  _isGrounded = false;
            _jumpGraceTimer = 0; // Reset jump grace timer after jumping
        }
        IsGrounded = VerticalVelocity == 0;
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





















    Vector3 newPositionY;
    private void HandleMove()
    { 
        if (VerticalVelocity > Gravity) //terminal velocity
        {
            VerticalVelocity += Gravity * _deltaTime;
        } 
        newPositionY = new Vector3(_newPosition.x, _newPosition.y + VerticalVelocity * _deltaTime, _newPosition.z);
        if (!IsMovable(newPositionY))
        {
            if (VerticalVelocity < 0) IsGrounded = true;
            VerticalVelocity = 0;
            Machine.transform.position = _newPosition;
        } 
        else 
        {
            Machine.transform.position = newPositionY;
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
