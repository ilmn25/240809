using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class NPCMovementInst : MonoBehaviour
{

    [SerializeField] public float SPEED_WALK = 6f;
    [SerializeField] private float SPEED_RUN = 8f;
    [SerializeField] private float ACCELERATION_TIME = 0.3f; // time to reach full speed
    [SerializeField] private float DECELERATION_TIME = 0.08f; // time to stop
    [SerializeField] private float SLIDE_DEGREE = 0.3f; //degree of slide against walkk when collide
    [SerializeField] private float GRAVITY = -40f;
    [SerializeField] private float JUMP_VELOCITY = 12f;


    private bool _isGrounded = false;
    private LayerMask _collisionLayer;
    private float _speedCurrent;
    private float _speedTarget;
    private float _speedAdjust;
    private Vector3 _newPosition;
    private Vector3 _direction = Vector3.zero;
    private Vector3 _directionBuffer = Vector3.zero;
    private float _verticalVelocity = 0f;
    private float _deltaTime;
    private Vector3[] _collider;
    private float _colliderRadius; 

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

    private void Start() {
        _collisionLayer = LayerMask.GetMask("Collision");

        CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
        Vector3 point1 = capsuleCollider.center + Vector3.up * (capsuleCollider.height / 2 - capsuleCollider.radius);
        Vector3 point2 = capsuleCollider.center + Vector3.down * (capsuleCollider.height / 2 - capsuleCollider.radius);
        _collider = new Vector3[] { point1, point2};
        _colliderRadius = capsuleCollider.radius;
        Destroy(capsuleCollider);
    }
      

    public void HandleMovementUpdate()
    { 
        if (transform.position.y < -1) transform.position = Lib.AddToVector(transform.position, 0, 40, 0);

        // _deltaTime = GameSystem._deltaTime;
        _deltaTime = GameStatic.GetDeltaTime();
        _newPosition = transform.position;

        HandleJump();

        if (_direction != Vector3.zero)
        {  
            //! speeding up to start
            _speedTarget = _isGrounded ? SPEED_WALK : SPEED_RUN;
            // _speedTarget = SPEED_WALK;
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
            _newPosition.z += _directionBuffer.y * _speedCurrent * _deltaTime;

            if (!IsMovable(_newPosition))
            {
                _speedCurrent = _speedCurrent/2;
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
 


 
    float testPosition;
    Vector3 testPositionA;
    Vector3 testPositionB;
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
                testPosition = transform.position.x + SLIDE_DEGREE * _direction.x * _speedCurrent * _deltaTime;
                testPositionA = new Vector3(testPosition, transform.position.y, transform.position.z);
                testPositionB = new Vector3(testPosition, transform.position.y, transform.position.z);
                testPositionA.z += -1 * _speedCurrent * _deltaTime;
                testPositionB.z += 1 * _speedCurrent * _deltaTime;
                if (IsMovable(testPositionA) && !IsMovable(testPositionB))
                {
                    _newPosition = testPositionA;
                }
                else if (!IsMovable(testPositionA) && IsMovable(testPositionB))
                {
                    _newPosition = testPositionB;
                }
            }
            else
            {
                testPosition = transform.position.z + SLIDE_DEGREE * _direction.z * _speedCurrent * _deltaTime;
                testPositionA = new Vector3(transform.position.x, transform.position.y, testPosition);
                testPositionB = new Vector3(transform.position.x, transform.position.y, testPosition);
                testPositionA.x += -1 * _speedCurrent * _deltaTime;
                testPositionB.x += 1 * _speedCurrent * _deltaTime;
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
    } 
   
    private Vector3 newPositionY;
    private void HandleMove()
    { 
        if (_verticalVelocity > GRAVITY) //terminal velocity
        {
            _verticalVelocity += GRAVITY * _deltaTime;
        } 
        newPositionY = new Vector3(_newPosition.x, _newPosition.y + _verticalVelocity * _deltaTime, _newPosition.z);
        if (!IsMovable(newPositionY))
        { 
            if (_verticalVelocity < 0) _isGrounded = true;
            _verticalVelocity = 0;
            transform.position = _newPosition;
        } 
        else
        {
            transform.position = newPositionY;
        } 
    }

    Collider[] tempCollisionArray = new Collider[1];
    int collisionCount; 
    private bool IsMovable(Vector3 _newPosition)
    {  
        // Define an array to store the results
        tempCollisionArray = new Collider[1]; 
        collisionCount = Physics.OverlapCapsuleNonAlloc(_newPosition + _collider[0], _newPosition + _collider[1], _colliderRadius, tempCollisionArray, _collisionLayer);

        return !(collisionCount > 0);
    }
} 

