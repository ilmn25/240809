using System.Collections;
using UnityEngine;

public class NPCAnimationInst : MonoBehaviour
{
    private Animator _animator;
    private GameObject _sprite;
    private NPCMovementInst _npcMovementInst;
    private int _flipDirection;
    private float _nextTrailTimer = 0f; // Time when the next trail should be created
    private Vector2 _relativeDirection;

    private float BOUNCE_SPEED = 1; // Speed of the bounce
    private float BOUNCE_RANGE = 0.12f; // Range of the bounce  
    private float TRAIL_FREQUENCY = 0.5f; // Frequency of the trail creation
    private float FLIP_DURATION = 0.06f; // Duration of the scaling effect 

    private int _currentScaleState = 0;
    private float _flipTimer = 0f;
    private Vector3 _originalScale;
    private Vector3 _flatScale;
    private Vector3 _targetScale; 

    void Awake()
    {
        _sprite = transform.Find("sprite").gameObject;
        _npcMovementInst = GetComponent<NPCMovementInst>();
        _animator = _sprite.GetComponent<Animator>();
        
        _targetScale = _sprite.transform.localScale; 
        _originalScale = _sprite.transform.localScale;
        _flatScale = new Vector3(0, _originalScale.y, 1);
    }
    
    bool isMoving;
    float newY;
    public void HandleAnimationUpdate()
    {
        // facing _relativeDirection 
        _relativeDirection = new Vector2(_npcMovementInst._direction.x, _npcMovementInst._direction.z);  
        _relativeDirection.Normalize(); 
        if (_relativeDirection != Vector2.zero)
        { 
            _relativeDirection.x = Mathf.Abs(_relativeDirection.x) < 0.1f ? 0 : Mathf.Sign(_relativeDirection.x);
            _relativeDirection.y = Mathf.Abs(_relativeDirection.y) < 0.1f ? 0 : Mathf.Sign(_relativeDirection.y);
            _relativeDirection = CameraStatic.GetRelativeDirection(_relativeDirection); 
            _animator.SetFloat("PosX", _relativeDirection.x);
            _animator.SetFloat("PosY", _relativeDirection.y);
        }

        isMoving = _npcMovementInst._speedCurrent > 0.35 && _npcMovementInst._isGrounded;
        _animator.SetBool("movementFlag", isMoving); // moving or idle
        if (isMoving)
        {
            // bounce 
            newY = Mathf.PingPong(Time.time * BOUNCE_SPEED, BOUNCE_RANGE);
            _sprite.transform.localPosition = new Vector3(_sprite.transform.localPosition.x, newY, _sprite.transform.localPosition.z);
            
            // smoke trail
            if (Time.time >= _nextTrailTimer)
            {
                SmokeParticleStatic.CreateSmokeParticle(transform.position, false);
                _nextTrailTimer = Time.time + TRAIL_FREQUENCY;
                // AudioSystem.PlaySFX(Resources.Load<AudioClip>($"audio/sfx/footstep/footstep{Random.Range(1, 3)}"), 0.3f);
            }
        } 
        else 
        {
            _sprite.transform.localPosition = new Vector3(_sprite.transform.localPosition.x, 0, _sprite.transform.localPosition.z);
        }

        // flip
        HandleFlipCheck();
        HandleScaling();
    }

    void HandleFlipCheck()
    {     
        if ((int)_relativeDirection.x != 0) 
        {
            if (Mathf.Sign((int)_animator.GetFloat("PosX")) != Mathf.Sign(_targetScale.x))
            {
                _flipDirection = (int)_animator.GetFloat("PosX");  
                _targetScale = new Vector3(Mathf.Sign(_flipDirection), _originalScale.y, _originalScale.z);
                _flipTimer = 0f;
                _currentScaleState = 1;
            }   
        } 
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
