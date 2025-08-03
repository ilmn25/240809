using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class NPCMovementModule : Module
{
    [SerializeField] public float SPEED_WALK = 4f;
    [SerializeField] private float SPEED_RUN = 8f;
    [SerializeField] private float ACCELERATION_TIME = 0.3f;  
    [SerializeField] private float DECELERATION_TIME = 0.08f; 
    [SerializeField] private float SLIDE_DEGREE = 0.3f; //against wall
    [SerializeField] private float GRAVITY = -40f;
    [SerializeField] private float JUMP_VELOCITY = 12f;
    [SerializeField] private float COLLISION_RADIUS = 0.3f; 
     
    public Vector3 GetDirection()
    {
        return _direction;
    }
    public float GetSpeed()
    {
        return _speedCurrent;
    }
    public Boolean IsGrounded()
    {
        return _isGrounded;
    }
    
    
    
    
    // variables
    private bool _isGrounded = false;
    
    private float _speedCurrent;
    private float _speedTarget;
    private float _speedAdjust;
    
    private Vector3 _newPosition;
    private Vector3 _previousPosition;
    
    private Vector3 _direction = Vector3.zero;
    private Vector3 _directionBuffer = Vector3.zero;
    
    private Vector3 _velocity = Vector3.zero;
    
    private float _deltaTime; 
    
    private float _testPosition;
    private Vector3 _testPositionA;
    private Vector3 _testPositionB;

    private Vector3 _tempPosition;
    
    private Collider[] _colliderArray = new Collider[1];
    
    public void KnockBack(Vector3 position, float force, bool isAway)
    {
        _velocity += (isAway? Machine.transform.position - position : Machine.transform.position + position).normalized * force;
    }

    public void HandleMovementUpdate(Vector3 direction)
    {
        _direction = direction;
        if (Machine.transform.position.y < -1) Machine.transform.position = Utility.AddToVector(Machine.transform.position, 0, 100, 0);

        // _deltaTime = GameSystem._deltaTime;
        _deltaTime = Utility.GetDeltaTime();
        _newPosition = Machine.transform.position;

        HandleJump();

        if (_direction != Vector3.zero)
        {  
            //! speeding up to start
            _speedTarget = _isGrounded ? SPEED_WALK : SPEED_RUN;
            _speedCurrent = Mathf.Lerp(_speedCurrent, _speedTarget, _deltaTime / ACCELERATION_TIME);
            _speedAdjust = (_direction.x != 0 && _direction.z != 0) ? 1 / 1.25f : 1; 

            _newPosition.x += _direction.x * _speedCurrent * _deltaTime * _speedAdjust;
            _newPosition.z += _direction.z * _speedCurrent * _deltaTime * _speedAdjust;

            if (!IsMovable(_newPosition))
            {
                HandleObstacle(_newPosition);
            } 
            _directionBuffer = _direction;
        }
        else if (_speedCurrent != 0)
        {
            //! slowing down to stop
            _speedCurrent = (_speedCurrent < 0.05f) ? 0f : Mathf.Lerp(_speedCurrent, 0, _deltaTime / DECELERATION_TIME);

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
        if (_isGrounded && _direction.y > 0)
        {
            _velocity.y = JUMP_VELOCITY; 
            _isGrounded = false;
        }
    }
 
    private void HandleObstacle(Vector3 position)
    {
        _newPosition = Machine.transform.position;
        //! go any possible direction when going diagonally against a wall
        if (_direction.x != 0 && IsMovable(new Vector3(position.x, Machine.transform.position.y, Machine.transform.position.z)))
        {
            _newPosition = new Vector3(position.x, Machine.transform.position.y, Machine.transform.position.z);
        }
        else if (_direction.z != 0 && IsMovable(new Vector3(Machine.transform.position.x, Machine.transform.position.y, position.z)))
        {
            _newPosition = new Vector3(Machine.transform.position.x, Machine.transform.position.y, position.z);
        }
        else
        {
            //! slide against wall if possible
            if (_direction.x != 0)
            {
                _testPosition = Machine.transform.position.x + SLIDE_DEGREE * _direction.x * _speedCurrent * _deltaTime;
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
                _testPosition = Machine.transform.position.z + SLIDE_DEGREE * _direction.z * _speedCurrent * _deltaTime;
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
        if (_velocity.y > GRAVITY) //terminal velocity
        {
            _velocity.y += GRAVITY * _deltaTime;
        } 
        
        _tempPosition = new Vector3(_newPosition.x + _velocity.x * _deltaTime, _newPosition.y, _newPosition.z);
        if (!IsMovable(_tempPosition))
            _velocity.x = 0; 
        else
            _newPosition = _tempPosition;
        
        _tempPosition = new Vector3(_newPosition.x, _newPosition.y, _newPosition.z + _velocity.z * _deltaTime);
        if (!IsMovable(_tempPosition))
            _velocity.z = 0; 
        else
            _newPosition = _tempPosition;
        
        _velocity.x = Mathf.MoveTowards(_velocity.x, 0f, 30 * Time.deltaTime);
        _velocity.z = Mathf.MoveTowards(_velocity.z, 0f, 30 * Time.deltaTime);
        
        _tempPosition = new Vector3(_newPosition.x, _newPosition.y + _velocity.y * _deltaTime, _newPosition.z);
        if (!IsMovable(_tempPosition))
        { 
            if (_velocity.y < 0) _isGrounded = true;
            _velocity.y = 0;
            Machine.transform.position = _newPosition;
        } 
        else
        {
            Machine.transform.position = _tempPosition;
        } 
        
        
        if (_direction != Vector3.zero && _previousPosition == Machine.transform.position)
        {
            Vector3 tempPosition = Utility.AddToVector(Machine.transform.position, 0, 0.1f, 0);
            if (IsMovable(tempPosition)) Machine.transform.position = tempPosition;
        }
        _previousPosition = Machine.transform.position;
    }
    
    private bool IsMovable(Vector3 newPosition)
    {
        _colliderArray = new Collider[1];

        Vector3 halfExtents = new Vector3(COLLISION_RADIUS, COLLISION_RADIUS, COLLISION_RADIUS);

        return !(Physics.OverlapBoxNonAlloc(newPosition + new Vector3(0, 0.15f, 0), halfExtents, _colliderArray, Quaternion.identity, Game.MaskStatic) > 0);
    } 
} 

