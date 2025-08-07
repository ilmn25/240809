using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class GroundMovementModule : MovementModule
{
    private readonly float _speed;
    private readonly float _speedAir;
    private readonly float _accelerationTime;
    private readonly float _decelerationTime; 
    private readonly float _gravity;
    private readonly float _jumpVelocity;
    private readonly float _collisionRadius;  
    private const float SlideDegree = 0.3f;
    
     
    private float _speedCurrent;
    private float _speedTarget;
    private float _speedAdjust;
    private Vector3 _newPosition;
    private Vector3 _previousPosition;
    private Vector3 _directionBuffer = Vector3.zero; 
    private float _deltaTime; 
    private float _testPosition;
    private Vector3 _testPositionA;
    private Vector3 _testPositionB;
    private Vector3 _tempPosition;
    
    private Collider[] _colliderArray = new Collider[1];
    
    public GroundMovementModule(
        float speed = 5f,
        float speedAir = 6f,
        float accelerationTime = 0.3f,
        float decelerationTime = 0.08f,
        float gravity = -40f,
        float jumpVelocity = 10f,
        float collisionRadius = 0.3f)
    {
        _speed = speed;
        _speedAir = speedAir;
        _accelerationTime = accelerationTime;
        _decelerationTime = decelerationTime;
        _gravity = gravity;
        _jumpVelocity = jumpVelocity;
        _collisionRadius = collisionRadius;
    }


    public override void Update()
    { 
        if (Machine.transform.position.y < -1) Machine.transform.position = Utility.AddToVector(Machine.transform.position, 0, 100, 0);
        
        if (!MapLoad.ActiveChunks.ContainsKey(World.GetChunkCoordinate(Machine.transform.position))) return;
        
        // _deltaTime = GameSystem._deltaTime;
        _deltaTime = Utility.GetDeltaTime();
        _newPosition = Machine.transform.position;

        HandleJump();

        if (StatusModule.Direction != Vector3.zero)
        {  
            //! speeding up to start
            _speedTarget = StatusModule.IsGrounded ? _speed : _speedAir;
            _speedCurrent = Mathf.Lerp(_speedCurrent, _speedTarget, _deltaTime / _accelerationTime);
            _speedAdjust = (StatusModule.Direction.x != 0 && StatusModule.Direction.z != 0) ? 1 / 1.25f : 1; 

            _newPosition.x += StatusModule.Direction.x * _speedCurrent * _deltaTime * _speedAdjust;
            _newPosition.z += StatusModule.Direction.z * _speedCurrent * _deltaTime * _speedAdjust;

            if (!IsMovable(_newPosition))
            {
                HandleObstacle(_newPosition);
            } 
            _directionBuffer = StatusModule.Direction;
        }
        else if (_speedCurrent != 0)
        {
            //! slowing down to stop
            _speedCurrent = (_speedCurrent < 0.05f) ? 0f : Mathf.Lerp(_speedCurrent, 0, _deltaTime / _decelerationTime);

            _newPosition.x += _directionBuffer.x * _speedCurrent * _deltaTime;
            _newPosition.z += _directionBuffer.z * _speedCurrent * _deltaTime;

            if (!IsMovable(_newPosition))
            {
                _speedCurrent /= 2;
                _newPosition = Machine.transform.position;
            }
        }
 
        HandleMove();  
    } 

    private void HandleJump()
    { 
        // if (_isGrounded && _direction.y > 0 && _npcPathFindInst._nextPointDistance < 2f)
        if (StatusModule.IsGrounded && StatusModule.Direction.y > 0)
        {
            StatusModule.Velocity.y = _jumpVelocity; 
            StatusModule.IsGrounded = false;
        }
    }
 
    private void HandleObstacle(Vector3 position)
    {
        _newPosition = Machine.transform.position;
        //! go any possible direction when going diagonally against a wall
        if (StatusModule.Direction.x != 0 && IsMovable(new Vector3(position.x, Machine.transform.position.y, Machine.transform.position.z)))
        {
            _newPosition = new Vector3(position.x, Machine.transform.position.y, Machine.transform.position.z);
        }
        else if (StatusModule.Direction.z != 0 && IsMovable(new Vector3(Machine.transform.position.x, Machine.transform.position.y, position.z)))
        {
            _newPosition = new Vector3(Machine.transform.position.x, Machine.transform.position.y, position.z);
        }
        else
        {
            //! slide against wall if possible
            if (StatusModule.Direction.x != 0)
            {
                _testPosition = Machine.transform.position.x + SlideDegree * StatusModule.Direction.x * _speedCurrent * _deltaTime;
                _testPositionA = new Vector3(_testPosition, Machine.transform.position.y, Machine.transform.position.z);
                _testPositionB = new Vector3(_testPosition, Machine.transform.position.y, Machine.transform.position.z);
                _testPositionA.z += -1 * _speedCurrent * _deltaTime;
                _testPositionB.z += 1 * _speedCurrent * _deltaTime;
                if (IsMovable(_testPositionA) && !IsMovable(_testPositionB))
                {
                    _newPosition = _testPositionA;
                }
                else if (!IsMovable(_testPositionA) && IsMovable(_testPositionB))
                {
                    _newPosition = _testPositionB;
                }
            }
            else
            {
                _testPosition = Machine.transform.position.z + SlideDegree * StatusModule.Direction.z * _speedCurrent * _deltaTime;
                _testPositionA = new Vector3(Machine.transform.position.x, Machine.transform.position.y, _testPosition);
                _testPositionB = new Vector3(Machine.transform.position.x, Machine.transform.position.y, _testPosition);
                _testPositionA.x += -1 * _speedCurrent * _deltaTime;
                _testPositionB.x += 1 * _speedCurrent * _deltaTime;
                if (IsMovable(_testPositionA) && !IsMovable(_testPositionB))
                {
                    _newPosition = _testPositionA;
                }
                else if (!IsMovable(_testPositionA) && IsMovable(_testPositionB))
                {
                    _newPosition = _testPositionB;
                }
            }
        }
    } 
    
    private void HandleMove()
    { 
        if (StatusModule.Velocity.y > _gravity) //terminal velocity
        {
            StatusModule.Velocity.y += _gravity * _deltaTime;
        } 
        
        _tempPosition = new Vector3(_newPosition.x + StatusModule.Velocity.x * _deltaTime, _newPosition.y, _newPosition.z);
        if (!IsMovable(_tempPosition))
            StatusModule.Velocity.x = 0; 
        else
            _newPosition = _tempPosition;
        
        _tempPosition = new Vector3(_newPosition.x, _newPosition.y, _newPosition.z + StatusModule.Velocity.z * _deltaTime);
        if (!IsMovable(_tempPosition))
            StatusModule.Velocity.z = 0; 
        else
            _newPosition = _tempPosition;
        
        StatusModule.Velocity.x = Mathf.MoveTowards(StatusModule.Velocity.x, 0f, 30 * Time.deltaTime);
        StatusModule.Velocity.z = Mathf.MoveTowards(StatusModule.Velocity.z, 0f, 30 * Time.deltaTime);
        
        _tempPosition = new Vector3(_newPosition.x, _newPosition.y + StatusModule.Velocity.y * _deltaTime, _newPosition.z);
        if (!IsMovable(_tempPosition))
        { 
            if (StatusModule.Velocity.y < 0) StatusModule.IsGrounded = true;
            StatusModule.Velocity.y = 0;
            Machine.transform.position = _newPosition;
        } 
        else
        {
            Machine.transform.position = _tempPosition;
        } 
        
        
        if (StatusModule.Direction != Vector3.zero && _previousPosition == Machine.transform.position)
        {
            Vector3 tempPosition = Utility.AddToVector(Machine.transform.position, 0, 0.1f, 0);
            if (IsMovable(tempPosition)) Machine.transform.position = tempPosition;
        }
        _previousPosition = Machine.transform.position;
    }
    
    private bool IsMovable(Vector3 newPosition)
    {
        _colliderArray = new Collider[1];

        Vector3 halfExtents = new Vector3(_collisionRadius, _collisionRadius, _collisionRadius);

        return !(Physics.OverlapBoxNonAlloc(newPosition + new Vector3(0, 0.15f, 0), halfExtents, _colliderArray, Quaternion.identity, Game.MaskStatic) > 0);
    } 
} 

