using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class GroundMovementModule : MovementModule
{ 
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
    
    private static Collider[] _colliderArray = new Collider[1];

    public override void Update()
    { 
        if (Machine.transform.position.y < -1) Machine.transform.position = Utility.AddToVector(Machine.transform.position, 0, 100, 0);
        
        if (!MapLoad.ActiveChunks.ContainsKey(World.GetChunkCoordinate(Machine.transform.position))) return;
        
        // _deltaTime = GameSystem._deltaTime;
        _deltaTime = Utility.GetDeltaTime();
        _newPosition = Machine.transform.position;

        HandleJump();

        if (Info.Direction != Vector3.zero)
        {  
            //! speeding up to start
            _speedTarget = Info.IsGrounded ? Info.SpeedGround : Info.SpeedAir;
            _speedCurrent = Mathf.Lerp(_speedCurrent, _speedTarget, _deltaTime / Info.AccelerationTime);
            _speedAdjust = (Info.Direction.x != 0 && Info.Direction.z != 0) ? 1 / 1.25f : 1; 

            _newPosition.x += Info.Direction.x * _speedCurrent * _deltaTime * _speedAdjust;
            _newPosition.z += Info.Direction.z * _speedCurrent * _deltaTime * _speedAdjust;

            if (!IsMovable(_newPosition))
            {
                HandleObstacle(_newPosition);
            } 
            _directionBuffer = Info.Direction;
        }
        else if (_speedCurrent != 0)
        {
            //! slowing down to stop
            _speedCurrent = (_speedCurrent < 0.05f) ? 0f : Mathf.Lerp(_speedCurrent, 0, _deltaTime / Info.DecelerationTime);

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
        if (Info.IsGrounded && Info.Direction.y > 0)
        {
            Info.Velocity.y = Info.JumpVelocity; 
            Info.IsGrounded = false;
        }
    }
 
    private void HandleObstacle(Vector3 position)
    {
        _newPosition = Machine.transform.position;
        //! go any possible direction when going diagonally against a wall
        if (Info.Direction.x != 0 && IsMovable(new Vector3(position.x, Machine.transform.position.y, Machine.transform.position.z)))
        {
            _newPosition = new Vector3(position.x, Machine.transform.position.y, Machine.transform.position.z);
        }
        else if (Info.Direction.z != 0 && IsMovable(new Vector3(Machine.transform.position.x, Machine.transform.position.y, position.z)))
        {
            _newPosition = new Vector3(Machine.transform.position.x, Machine.transform.position.y, position.z);
        }
        else
        {
            //! slide against wall if possible
            if (Info.Direction.x != 0)
            {
                _testPosition = Machine.transform.position.x + SlideDegree * Info.Direction.x * _speedCurrent * _deltaTime;
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
                _testPosition = Machine.transform.position.z + SlideDegree * Info.Direction.z * _speedCurrent * _deltaTime;
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
        if (Info.Velocity.y > Info.Gravity) //terminal velocity
        {
            Info.Velocity.y += Info.Gravity * _deltaTime;
        } 
        
        _tempPosition = new Vector3(_newPosition.x + Info.Velocity.x * _deltaTime, _newPosition.y, _newPosition.z);
        if (!IsMovable(_tempPosition))
            Info.Velocity.x = 0; 
        else
            _newPosition = _tempPosition;
        
        _tempPosition = new Vector3(_newPosition.x, _newPosition.y, _newPosition.z + Info.Velocity.z * _deltaTime);
        if (!IsMovable(_tempPosition))
            Info.Velocity.z = 0; 
        else
            _newPosition = _tempPosition;
        
        Info.Velocity.x = Mathf.MoveTowards(Info.Velocity.x, 0f, 30 * Time.deltaTime);
        Info.Velocity.z = Mathf.MoveTowards(Info.Velocity.z, 0f, 30 * Time.deltaTime);
        
        _tempPosition = new Vector3(_newPosition.x, _newPosition.y + Info.Velocity.y * _deltaTime, _newPosition.z);
        if (!IsMovable(_tempPosition))
        { 
            if (Info.Velocity.y < 0) Info.IsGrounded = true;
            Info.Velocity.y = 0;
            Machine.transform.position = _newPosition;
        } 
        else
        {
            Machine.transform.position = _tempPosition;
        } 
        
        
        if (Info.Direction != Vector3.zero && _previousPosition == Machine.transform.position)
        {
            Vector3 tempPosition = Utility.AddToVector(Machine.transform.position, 0, 0.1f, 0);
            if (IsMovable(tempPosition)) Machine.transform.position = tempPosition;
        }
        _previousPosition = Machine.transform.position;
    }
    
    private bool IsMovable(Vector3 newPosition)
    {
        // if (!Scene.InPlayerChunkRange(World.GetChunkCoordinate(newPosition), Scene.LogicDistance)) return false;  
        
        return !(Physics.OverlapBoxNonAlloc(newPosition + new Vector3(0, 0.15f, 0), Vector3.one * Info.CollisionRadius, _colliderArray, Quaternion.identity, Game.MaskStatic) > 0);
    } 
} 

