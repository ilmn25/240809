using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class GroundMovementModule : MovementModule
{ 
    private const float SlideDegree = 0.3f;
     
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
    
    private static readonly Collider[] ColliderArray = new Collider[1];
    
    public override void Update()
    { 
        if (Info.Health <= 0) return;
        if (Machine.transform.position.y < -1) Machine.transform.position = Helper.AddToVector(Machine.transform.position, 0, 100, 0);
        
        _deltaTime = Helper.GetDeltaTime();
        if (!MapLoad.ActiveChunks.ContainsKey(World.GetChunkCoordinate(Machine.transform.position))) {
            Machine.transform.position += Info.Direction * (_deltaTime * Info.SpeedTarget);
            return;
        }
        _newPosition = Machine.transform.position;

        HandleJump(); 
        if (Info.Direction != Vector3.zero)
        {  
            //! speeding up to start
            Info.SpeedCurrent = Mathf.Lerp(Info.SpeedCurrent, Info.SpeedTarget, _deltaTime / Info.AccelerationTime);
            // if (Info.IsPlayer)Utility.Log(Info.SpeedTarget, Info.Direction);
            if (Info.Direction.x != 0 && Info.Direction.z != 0)
            {
                _speedAdjust = 1 / Mathf.Sqrt(2); // This is equivalent to 1 / 1.41421
            } else _speedAdjust = 1;
            _newPosition.x += Info.Direction.x * Info.SpeedCurrent * _deltaTime * _speedAdjust;
            _newPosition.z += Info.Direction.z * Info.SpeedCurrent * _deltaTime * _speedAdjust;

            if (!IsMovable(_newPosition))
            {
                HandleObstacle(_newPosition);
            } 
            _directionBuffer = Info.Direction;
        }
        else if (Info.SpeedCurrent != 0)
        {
            //! slowing down to stop
            Info.SpeedCurrent = (Info.SpeedCurrent < 0.05f) ? 0f : Mathf.Lerp(Info.SpeedCurrent, 0, _deltaTime / Info.DecelerationTime);

            _newPosition.x += _directionBuffer.x * Info.SpeedCurrent * _deltaTime;
            _newPosition.z += _directionBuffer.z * Info.SpeedCurrent * _deltaTime;

            if (!IsMovable(_newPosition))
            {
                Info.SpeedCurrent /= 2;
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
                _testPosition = Machine.transform.position.x + SlideDegree * Info.Direction.x * Info.SpeedCurrent * _deltaTime;
                _testPositionA = new Vector3(_testPosition, Machine.transform.position.y, Machine.transform.position.z);
                _testPositionB = new Vector3(_testPosition, Machine.transform.position.y, Machine.transform.position.z);
                _testPositionA.z += -1 * Info.SpeedCurrent * _deltaTime;
                _testPositionB.z += 1 * Info.SpeedCurrent * _deltaTime;
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
                _testPosition = Machine.transform.position.z + SlideDegree * Info.Direction.z * Info.SpeedCurrent * _deltaTime;
                _testPositionA = new Vector3(Machine.transform.position.x, Machine.transform.position.y, _testPosition);
                _testPositionB = new Vector3(Machine.transform.position.x, Machine.transform.position.y, _testPosition);
                _testPositionA.x += -1 * Info.SpeedCurrent * _deltaTime;
                _testPositionB.x += 1 * Info.SpeedCurrent * _deltaTime;
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
        
        
        // if (Info.Direction != Vector3.zero && _previousPosition == Machine.transform.position)
        // {
        //     Debug.Log("aaa");
        //     Vector3 tempPosition = Utility.AddToVector(Machine.transform.position, 0, 0.1f, 0);
        //     if (IsMovable(tempPosition)) Machine.transform.position = tempPosition; 
        // }
        // _previousPosition = Machine.transform.position;
    }
    
    private readonly Vector3 _colliderSize = new Vector3(0.35f, 0.35f, 0.25f);  
    private readonly Vector3 _colliderCenter = new Vector3(0, 0.35f, 0);  
    private bool IsMovable(Vector3 newPosition)
    {
        // if (!Scene.InPlayerChunkRange(World.GetChunkCoordinate(newPosition), Scene.LogicDistance)) return false;  
        
        return !(Physics.OverlapBoxNonAlloc(newPosition + _colliderCenter, 
            _colliderSize, ColliderArray, Quaternion.identity, 
            Game.MaskStatic) > 0);
    } 
} 

