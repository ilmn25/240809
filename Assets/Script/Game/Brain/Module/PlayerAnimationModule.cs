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
    private PlayerStatusModule _playerStatusModule;
    private int _flipDirection;
    private float _nextTrailTimer = 0f;
    private int _currentScaleState = 0;
    private float _flipTimer = 0f;
    private Vector3 _originalScale;
    private Vector3 _flatScale;
    private Vector3 _targetScale; 
    private float _previousY = 1;
    private Vector2Int _animDirection = Vector2Int.zero;

    public override void Initialize()
    {
        _playerMovementModule = Machine.GetModule<PlayerMovementModule>();
        _playerStatusModule = Machine.GetModule<PlayerStatusModule>();
        
        _targetScale = _playerStatusModule.Sprite.localScale; 
        _originalScale = _playerStatusModule.Sprite.localScale;
        _flatScale = new Vector3(0, _originalScale.y, 1);

        ViewPort.UpdateOrbitRotate += UpdateOrbit;
    }

    void UpdateOrbit()
    { 
        Machine.transform.rotation = ViewPort.CurrentRotation;
    }

    public override void Update()
    {
        if (Game.GameState != GameState.Playing) return;
        UpdateDirection();
        HandleBounceAndTrail();
        HandleFlipCheck();
        HandleScaling();
    } 

    void UpdateDirection()
    {
        if (Inventory.CurrentItemData != null)
        {
            _animDirection = new Vector2Int((int)Mathf.Sign(_playerStatusModule.TargetScreenDir.x), 0);
            TrackMouse();
        }
        else
        {
            if (_playerMovementModule.RawInput != Vector2.zero)
            {
                _animDirection = new Vector2Int(
                    Mathf.RoundToInt(_playerMovementModule.RawInput.x),
                    Mathf.RoundToInt(_playerMovementModule.RawInput.y)
                );
            } 
        }
    }

    private void TrackMouse()
    {
        float angle = Mathf.Atan2(_playerStatusModule.TargetScreenDir.y, 
            _playerStatusModule.TargetScreenDir.x) * Mathf.Rad2Deg;

        if (angle > 90)
            angle = 180 - angle;
        else if (angle < -90)
            angle = -angle - 180;

        float z = Mathf.Lerp(-0.45f, 0.45f, (angle + 90) / 180);
        if (z is > 0f and <= 0.12f)
            z = 0.12f;
        else if (z is < 0f and >= -0.11f)
            z = -0.11f; 
        _playerStatusModule.SpriteToolTrack.localPosition = new Vector3(0, 0.3f, z);
        _playerStatusModule.SpriteToolTrack.localRotation = Quaternion.Euler(80, 0, (angle + 360) % 360);
    }
    
    void HandleBounceAndTrail()
    {
        bool isMoving = _playerMovementModule.SpeedCurrent > 0.35 && _playerMovementModule.IsGrounded;

        if (isMoving)
        {
            float newY = Mathf.PingPong(Time.time * BounceSpeed, BounceRange);
            _playerStatusModule.Sprite.localPosition = new Vector3(_playerStatusModule.Sprite.localPosition.x, newY, _playerStatusModule.Sprite.localPosition.z);

            if (Time.time >= _nextTrailTimer)
            {
                SmokeParticleHandler.CreateSmokeParticle(Machine.transform.position, true);
                _nextTrailTimer = Time.time + TrailFrequency;
                Audio.PlaySFX($"footstep_{Random.Range(1, 3)}", 0.3f);
            }
        }
        else
        {
            _playerStatusModule.Sprite.localPosition = new Vector3(_playerStatusModule.Sprite.localPosition.x, 0, _playerStatusModule.Sprite.localPosition.z);
        }
    }

    void HandleFlipCheck()
    {
        if (_animDirection.x != 0)
        {
            if (Mathf.Sign(_animDirection.x) != Mathf.Sign(_targetScale.x))
            {
                _flipDirection = _animDirection.x;
                _targetScale = new Vector3(Mathf.Sign(_flipDirection), _originalScale.y, _originalScale.z);
                _flipTimer = 0f;
                _currentScaleState = 1;
            }
        }
        else
        {
            if (_animDirection.y != (int)_previousY && Mathf.Sign(_animDirection.y) != Mathf.Sign(_targetScale.x))
            {
                _flipDirection = _animDirection.y;
                _targetScale = new Vector3(Mathf.Sign(_flipDirection), _originalScale.y, _originalScale.z);
                _flipTimer = 0f;
                _currentScaleState = 1;
            }
        }

        _previousY = _animDirection.y;
    }

    void HandleScaling()
    {
        if (_currentScaleState == 1)
        {
            _flipTimer += Time.deltaTime;
            _playerStatusModule.Sprite.localScale = Vector3.Lerp(_originalScale, _flatScale, _flipTimer / FlipDuration);

            if (_flipTimer >= FlipDuration)
            {
                _playerStatusModule.Sprite.localScale = _flatScale;
                _flipTimer = 0f;
                _currentScaleState = 2;
            }
        }
        else if (_currentScaleState == 2)
        {
            _flipTimer += Time.deltaTime;
            _playerStatusModule.Sprite.localScale = Vector3.Lerp(_flatScale, _targetScale, _flipTimer / FlipDuration);

            if (_flipTimer >= FlipDuration)
            {
                _playerStatusModule.Sprite.localScale = _targetScale;
                _currentScaleState = 0;
            }
        }
    }
}
