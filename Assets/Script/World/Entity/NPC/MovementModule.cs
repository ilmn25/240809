using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class MovementModule : MonoBehaviour
{
    [SerializeField] public float SPEED_WALK = 6f;
    [SerializeField] private float SPEED_RUN = 8f;
    [SerializeField] private float ACCELERATION_TIME = 0.3f;  
    [SerializeField] private float DECELERATION_TIME = 0.08f; 
    [SerializeField] private float SLIDE_DEGREE = 0.3f; //against wall
    [SerializeField] private float GRAVITY = -40f;
    [SerializeField] private float JUMP_VELOCITY = 12f;
    [SerializeField] private float COLLISION_RADIUS = 0.3f; 
    
    public void SetDirection(Vector3 dir)
    {
        _direction = dir;
    }
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
    
    private float _verticalVelocity = 0f;
    
    private float _deltaTime; 
    
    private float _testPosition;
    private Vector3 _testPositionA;
    private Vector3 _testPositionB;

    private Vector3 _newPositionY;
    
    private Collider[] _colliderArray = new Collider[1];
    
    public void HandleMovementUpdate(bool isRoam = false)
    { 
        if (transform.position.y < -1) transform.position = Lib.AddToVector(transform.position, 0, 100, 0);

        // _deltaTime = GameSystem._deltaTime;
        _deltaTime = GameStatic.GetDeltaTime();
        _newPosition = transform.position;

        HandleJump();

        if (_direction != Vector3.zero)
        {  
            //! speeding up to start
            _speedTarget = _isGrounded ? isRoam? SPEED_WALK * 0.7f : SPEED_WALK : SPEED_RUN;
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
                _newPosition = transform.position;
            }
        }
 
        HandleMove(); 
    } 

    private void HandleJump()
    { 
        // if (_isGrounded && _direction.y > 0 && _npcPathFindInst._nextPointDistance < 2f)
        if (_isGrounded && _direction.y > 0)
        {
            _verticalVelocity = JUMP_VELOCITY; 
            _isGrounded = false;
        }  
        // _isGrounded = _verticalVelocity == 0;
    }
  
    private void HandleObstacle(Vector3 position)
    {
        _newPosition = transform.position;
        //! go any possible direction when going diagonally against a wall
        if (_direction.x != 0 && IsMovable(new Vector3(position.x, transform.position.y, transform.position.z)))
        {
            _newPosition = new Vector3(position.x, transform.position.y, transform.position.z);
        }
        else if (_direction.z != 0 && IsMovable(new Vector3(transform.position.x, transform.position.y, position.z)))
        {
            _newPosition = new Vector3(transform.position.x, transform.position.y, position.z);
        }
        else
        {
            //! slide against wall if possible
            if (_direction.x != 0)
            {
                _testPosition = transform.position.x + SLIDE_DEGREE * _direction.x * _speedCurrent * _deltaTime;
                _testPositionA = new Vector3(_testPosition, transform.position.y, transform.position.z);
                _testPositionB = new Vector3(_testPosition, transform.position.y, transform.position.z);
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
                _testPosition = transform.position.z + SLIDE_DEGREE * _direction.z * _speedCurrent * _deltaTime;
                _testPositionA = new Vector3(transform.position.x, transform.position.y, _testPosition);
                _testPositionB = new Vector3(transform.position.x, transform.position.y, _testPosition);
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
        if (_verticalVelocity > GRAVITY) //terminal velocity
        {
            _verticalVelocity += GRAVITY * _deltaTime;
        } 
        _newPositionY = new Vector3(_newPosition.x, _newPosition.y + _verticalVelocity * _deltaTime, _newPosition.z);
        if (!IsMovable(_newPositionY))
        { 
            if (_verticalVelocity < 0) _isGrounded = true;
            _verticalVelocity = 0;
            transform.position = _newPosition;
        } 
        else
        {
            transform.position = _newPositionY;
        } 
        
        
        if (_direction != Vector3.zero && _previousPosition == transform.position)
        {
            Vector3 tempPosition = Lib.AddToVector(transform.position, 0, 0.1f, 0);
            if (IsMovable(tempPosition)) transform.position = tempPosition;
        }
        _previousPosition = transform.position;
    }
    
    // private bool IsMovable(Vector3 newPosition)
    // {
    //     _colliderArray = new Collider[1];
    //
    //     Vector3 halfExtents = new Vector3(COLLISION_RADIUS, COLLISION_RADIUS, COLLISION_RADIUS);
    //
    //     return !(Physics.OverlapBoxNonAlloc(newPosition + new Vector3(0, 0.15f, 0), halfExtents, _colliderArray, Quaternion.identity, Game.LayerCollide) > 0);
    // }
    private bool IsMovable(Vector3 newPosition)
    {
        // Define an array to store the results
        _colliderArray = new Collider[1];

        return !(Physics.OverlapSphereNonAlloc(newPosition, COLLISION_RADIUS, _colliderArray, Game.LayerCollide) > 0);
    }
} 

