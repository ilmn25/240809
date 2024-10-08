using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerAnimationSystem : MonoBehaviour
{ 
    private Animator _animator;
    private GameObject _sprite;
    private PlayerMovementSystem _playerMovementSystem; 
    private int _flipDirection;
    private float _nextTrailTimer = 0f; // Time when the next trail should be created

    private float BOUNCE_SPEED = 1.65f; // Speed of the bounce
    private float BOUNCE_RANGE = 0.15f; // Range of the bounce 
    // private float TRAIL_DURATION = 0.5f; // Duration of the trail
    private float TRAIL_FREQUENCY = 0.15f; // Frequency of the trail creation
    private float FLIP_DURATION = 0.05f; // Duration of the scaling effect 

    private int _currentScaleState = 0;
    private float _flipTimer = 0f;
    private Vector3 _originalScale;
    private Vector3 _flatScale;
    private Vector3 _targetScale; 
    private float _previousY = 1;

    void Awake()
    {
        _sprite = transform.Find("sprite").gameObject;
        _playerMovementSystem = GetComponent<PlayerMovementSystem>();
        _animator = _sprite.GetComponent<Animator>();

        _targetScale = _sprite.transform.localScale; 
        _originalScale = _sprite.transform.localScale;
        _flatScale = new Vector3(0, _originalScale.y, 1);

        CameraSystem.OnOrbitRotate += UpdateOrbit;
    }

    void OnDestroy()
    {
        CameraSystem.OnOrbitRotate -= UpdateOrbit; 
    }   
 
    void UpdateOrbit()
    { 
        transform.rotation = CameraSystem._currentRotation;
    }

    public void HandleAnimationUpdate()
    {
        // facing direction 
        if (_playerMovementSystem._rawInput != Vector2.zero){
            _animator.SetFloat("PosX", _playerMovementSystem._rawInput.x);
            _animator.SetFloat("PosY", _playerMovementSystem._rawInput.y);
        } 

        bool isMoving = _playerMovementSystem._speedCurrent > 0.35 && _playerMovementSystem._isGrounded;
        _animator.SetBool("movementFlag", isMoving); // moving or idle
        if (isMoving)
        {
            // bounce 
            float newY = Mathf.PingPong(Time.time * BOUNCE_SPEED, BOUNCE_RANGE);
            _sprite.transform.localPosition = new Vector3(_sprite.transform.localPosition.x, newY, _sprite.transform.localPosition.z);
            
            // smoke trail
            if (Time.time >= _nextTrailTimer)
            { 
                SmokeParticleSystem.CreateSmokeParticle(transform.position, true);
                _nextTrailTimer = Time.time + TRAIL_FREQUENCY;
                AudioSystem.PlaySFX(Resources.Load<AudioClip>($"audio/sfx/footstep/footstep{Random.Range(1, 3)}"), 0.3f);
            }
        } else _sprite.transform.localPosition = new Vector3(_sprite.transform.localPosition.x, 0, _sprite.transform.localPosition.z);


        HandleFlipCheck();
        HandleScaling();
    }

    void HandleFlipCheck()
    {     
        if ((int)_playerMovementSystem._rawInput.x != 0) 
        {
            if (Mathf.Sign((int)_animator.GetFloat("PosX")) != Mathf.Sign(_targetScale.x))
            {
                _flipDirection = (int)_animator.GetFloat("PosX");  
                _targetScale = new Vector3(Mathf.Sign(_flipDirection), _originalScale.y, _originalScale.z);
                _flipTimer = 0f;
                _currentScaleState = 1;
            }   
        } 
        else  
        { 
            if (_animator.GetFloat("PosY") != _previousY && Mathf.Sign((int)_animator.GetFloat("PosY")) != Mathf.Sign(_targetScale.x)) 
            {  
                _flipDirection = (int)_animator.GetFloat("PosY");  
                _targetScale = new Vector3(Mathf.Sign(_flipDirection), _originalScale.y, _originalScale.z);
                _flipTimer = 0f;
                _currentScaleState = 1;
            }
        }
        _previousY = _animator.GetFloat("PosY"); 
    }

    void HandleScaling()
    {
        if (_currentScaleState == 1)
        {
            _flipTimer += Time.deltaTime;
            _sprite.transform.localScale = Vector3.Lerp(_originalScale, _flatScale, _flipTimer / FLIP_DURATION);

            if (_flipTimer >= FLIP_DURATION)
            {
                _sprite.transform.localScale = _flatScale;
                _flipTimer = 0f; 
                _currentScaleState = 2;
            }
        }
        else if (_currentScaleState == 2)
        {
            _flipTimer += Time.deltaTime;
            _sprite.transform.localScale = Vector3.Lerp(_flatScale, _targetScale, _flipTimer / FLIP_DURATION);

            if (_flipTimer >= FLIP_DURATION)
            {
                _sprite.transform.localScale = _targetScale;
                _currentScaleState = 0;
            }
        }
    }
}
