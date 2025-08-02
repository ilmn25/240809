using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerAnimationModule : Module
{ 
    private const float BounceSpeed = 1.65f;
    private const float BounceRange = 0.15f;
    private const float TrailFrequency = 0.15f;
    public const float FlipDuration = 0.05f;
    
    private PlayerMovementModule _playerMovementModule;
    private Animator _animator;
    private GameObject _sprite;
    private int _flipDirection;
    private float _nextTrailTimer = 0f;
    private int _currentScaleState = 0;
    private float _flipTimer = 0f;
    private Vector3 _originalScale;
    private Vector3 _flatScale;
    private Vector3 _targetScale; 
    private float _previousY = 1;

    public override void Initialize()
    {
        _playerMovementModule = Machine.GetModule<PlayerMovementModule>();
        _sprite = Machine.transform.Find("sprite").gameObject;
        _animator = _sprite.transform.Find("char").GetComponent<Animator>();

        _targetScale = _sprite.transform.localScale; 
        _originalScale = _sprite.transform.localScale;
        _flatScale = new Vector3(0, _originalScale.y, 1);

        ViewPort.UpdateOrbitRotate += UpdateOrbit;
    }
 
    void UpdateOrbit()
    { 
        Machine.transform.rotation = ViewPort.CurrentRotation;
    }

    public void HandleAnimationUpdate()
    {
        // facing direction 
        if (_playerMovementModule.RawInput != Vector2.zero){
            _animator.SetFloat("PosX", _playerMovementModule.RawInput.x);
            _animator.SetFloat("PosY", _playerMovementModule.RawInput.y);
        } 

        bool isMoving = _playerMovementModule.SpeedCurrent > 0.35 && _playerMovementModule.IsGrounded;
        _animator.SetBool("movementFlag", isMoving); // moving or idle
        if (isMoving)
        {
            // bounce 
            float newY = Mathf.PingPong(Time.time * BounceSpeed, BounceRange);
            _sprite.transform.localPosition = new Vector3(_sprite.transform.localPosition.x, newY, _sprite.transform.localPosition.z);
            
            // smoke trail
            if (Time.time >= _nextTrailTimer)
            { 
                SmokeParticleHandler.CreateSmokeParticle(Machine.transform.position, true);
                _nextTrailTimer = Time.time + TrailFrequency;
                Audio.PlaySFX($"footstep_{Random.Range(1, 3)}", 0.3f);
            }
        } else _sprite.transform.localPosition = new Vector3(_sprite.transform.localPosition.x, 0, _sprite.transform.localPosition.z);


        HandleFlipCheck();
        HandleScaling();
    }

    void HandleFlipCheck()
    {     
        if ((int)_playerMovementModule.RawInput.x != 0) 
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
            _sprite.transform.localScale = Vector3.Lerp(_originalScale, _flatScale, _flipTimer / FlipDuration);

            if (_flipTimer >= FlipDuration)
            {
                _sprite.transform.localScale = _flatScale;
                _flipTimer = 0f; 
                _currentScaleState = 2;
            }
        }
        else if (_currentScaleState == 2)
        {
            _flipTimer += Time.deltaTime;
            _sprite.transform.localScale = Vector3.Lerp(_flatScale, _targetScale, _flipTimer / FlipDuration);

            if (_flipTimer >= FlipDuration)
            {
                _sprite.transform.localScale = _targetScale;
                _currentScaleState = 0;
            }
        }
    }
}
