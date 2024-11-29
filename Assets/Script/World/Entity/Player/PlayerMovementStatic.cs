using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.AI;
public class PlayerMovementStatic : MonoBehaviour
{ 
    public static PlayerMovementStatic Instance { get; private set; }  

    [HideInInspector] public float _verticalVelocity = 0f; 
    [HideInInspector] public bool _isGrounded = false;  
    [HideInInspector] public Vector2 _rawInput;  
    private Vector2 _input = Vector2.zero;
    private List<Vector2> _inputBuffer = new List<Vector2>();
    [HideInInspector] public float _speedCurrent; 
    private float _speedTarget;
    private Vector3 _newPosition;  
    private int _inputBufferCount = 4;
    private float _jumpGraceTimer;
    private float _coyoteTimer; 
    private float _deltaTime;  
    private float _speedAdjust;

    [SerializeField] private float SPEED_WALK = 6.5f;
    [SerializeField] private float SPEED_RUN = 8f;
    [SerializeField] private float ACCELERATION_TIME = 0.2f; // time to reach full speed
    [SerializeField] private float DECELERATION_TIME = 0.08f; // time to stop
    [SerializeField] private float SLIDE_DEGREE = 0.3f; //degree of slide against walkk when collide
    [SerializeField] private float GRAVITY = -40f;
    [SerializeField] private float JUMP_VELOCITY = 12f;
    [SerializeField] private float HOLD_VELOCITY = 0.05f;
    [SerializeField] private float CLAMP_VELOCITY = 10f;
    [SerializeField] private float JUMP_GRACE_TIME = 0.1f; // Time allowed to jump before landing
    [SerializeField] private float COYOTE_TIME = 0.1f; // Time allowed to jump after leaving ground

    BoxCollider boxCollider;

    void Awake()
    {
        Instance = this; 
        boxCollider = GetComponent<BoxCollider>(); 
        boxCollider.enabled = false;
        boxColliderSize = boxCollider.size / 2; // Recalculate halfSize here

    }





    float orbitRotation;
    float cosAngle;
    float sinAngle;
    public void HandleInput()
    {
        _rawInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (_rawInput == Vector2.zero)
        {
            _input = Vector2.zero;
            return;
        }
        orbitRotation = Mathf.Deg2Rad * -CameraStatic._orbitRotation;
        cosAngle = Mathf.Cos(orbitRotation);
        sinAngle = Mathf.Sin(orbitRotation);
        _input.x = _rawInput.x * cosAngle - _rawInput.y * sinAngle;
        _input.y = _rawInput.x * sinAngle + _rawInput.y * cosAngle; 
    }
  
    public void HandleMovementUpdate()
    {  
        
        _deltaTime = GameStatic.GetDeltaTime();
        _newPosition = transform.position;

        // get input
        HandleInput();
        // handle jumping and GRAVITY
        HandleJump();

        if (_input != Vector2.zero)
        {
            // speeding up to start
            if (Input.GetKey(KeyCode.LeftShift) && PlayerStatusStatic._stamina > 1)
            {
                _speedTarget = SPEED_RUN;
                PlayerStatusStatic._stamina -= 0.1f;
            }
            else
            {
                _speedTarget = SPEED_WALK;
                PlayerStatusStatic._stamina += 0.1f;
            }
            
            _speedCurrent = Mathf.Lerp(_speedCurrent, _speedTarget, _deltaTime / ACCELERATION_TIME);

            _speedAdjust = 1f; 
            if (_rawInput.x != 0 && _rawInput.y != 0)
            {
                _speedAdjust = 1 / Mathf.Sqrt(2); // This is equivalent to 1 / 1.41421
            }

            _newPosition.x += _input.x * _speedCurrent * _deltaTime * _speedAdjust;
            _newPosition.z += _input.y * _speedCurrent * _deltaTime * _speedAdjust;

            if (!IsMovable(_newPosition))
            {
                HandleObstacle(_newPosition);
            } 
 
            // Remove the first Vector2 from the list if it exceeds the desired size
            _inputBuffer.Add(_input); 
            if (_inputBuffer.Count > _inputBufferCount) _inputBuffer.RemoveAt(0);
        }
        else if (_speedCurrent != 0)
        {
            // slowing down to stop
            _speedCurrent = (_speedCurrent < 0.05f) ? 0f : Mathf.Lerp(_speedCurrent, 0, _deltaTime / DECELERATION_TIME);

            _newPosition.x += _inputBuffer[0].x * _speedCurrent * _deltaTime;
            _newPosition.z += _inputBuffer[0].y * _speedCurrent * _deltaTime;

            if (!IsMovable(_newPosition))
            {
                _speedCurrent = _speedCurrent/2;
                _newPosition = transform.position;
            }
        }

        HandleMove(); 
    }


    private bool fly;

    private void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.I)) fly = !fly;
        if (fly && Input.GetKeyDown(KeyCode.Space))
        {
            _verticalVelocity = JUMP_VELOCITY;
        }
        
        
        if ( _isGrounded)
        {
            _coyoteTimer = COYOTE_TIME; // Reset coyote timer when grounded
        }
        else
        {
            _coyoteTimer -= _deltaTime; // Decrease coyote timer when not grounded
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            _jumpGraceTimer = JUMP_GRACE_TIME; // Reset jump grace timer when jump key is pressed
        }
        else if (Input.GetKey(KeyCode.Space) && _verticalVelocity > CLAMP_VELOCITY)
        {
            _verticalVelocity += HOLD_VELOCITY;
        }
        else
        {
            _jumpGraceTimer -= _deltaTime; // Decrease jump grace timer
        }

        if ((_isGrounded || _coyoteTimer > 0) && _jumpGraceTimer > 0)
        {
            _verticalVelocity = JUMP_VELOCITY;
            //  _isGrounded = false;
            _jumpGraceTimer = 0; // Reset jump grace timer after jumping
        }
        _isGrounded = _verticalVelocity == 0;
    }





















    float testPosition;
    Vector3 testPositionA;
    Vector3 testPositionB;
    private void HandleObstacle(Vector3 position)
    {
        _newPosition = transform.position;
        //! go any possible direction when going diagonally against a wall
        if (_input.x != 0 && IsMovable(new Vector3(position.x, transform.position.y, transform.position.z)))
        { 
            _newPosition = new Vector3(position.x, transform.position.y, transform.position.z);
        }
        else if (_input.y != 0 && IsMovable(new Vector3(transform.position.x, transform.position.y, position.z)))
        { 
            _newPosition = new Vector3(transform.position.x, transform.position.y, position.z);
        }
        else
        { 
            //! slide against wall if possible
            if (_input.x != 0)
            {
                testPosition = transform.position.x + SLIDE_DEGREE * _input.x * _speedCurrent * _deltaTime;
                testPositionA = new Vector3(testPosition, transform.position.y, transform.position.z);
                testPositionB = new Vector3(testPosition, transform.position.y, transform.position.z);
                testPositionA.z += -1 * _speedCurrent * _deltaTime;
                testPositionB.z += 1 * _speedCurrent * _deltaTime; 
            }
            else
            {
                testPosition = transform.position.z + SLIDE_DEGREE * _input.y * _speedCurrent * _deltaTime;
                testPositionA = new Vector3(transform.position.x, transform.position.y, testPosition);
                testPositionB = new Vector3(transform.position.x, transform.position.y, testPosition);
                testPositionA.x += -1 * _speedCurrent * _deltaTime;
                testPositionB.x += 1 * _speedCurrent * _deltaTime; 
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
    Vector3 boxColliderSize; // Recalculate halfSize here
    int collisionCount;
    private bool IsMovable(Vector3 _newPosition)
    {
        // Define an array to store the results  
        collisionCount = Physics.OverlapBoxNonAlloc(_newPosition + boxCollider.center, boxColliderSize, tempCollisionArray, Quaternion.identity, Game.LayerCollide);
        return !(collisionCount > 0);
    }
 
}
