using System.Collections;
using UnityEngine;

public class GroundAnimationModule : Module
{
    private Transform _spriteTransform;
    private SpriteRenderer _spriteRenderer;
    private MobStatusModule _mobStatusModule;
    private int _flipDirection;
    private float _nextTrailTimer = 0f;
    private Vector2Int _animDirection = Vector2Int.zero;

    private const float BounceSpeed = 1f;
    private const float BounceRange = 0.12f;
    private const float TrailFrequency = 0.5f;
    private const float FlipDuration = 0.06f;

    private int _currentScaleState;
    private float _flipTimer;
    private Vector3 _originalScale;
    private Vector3 _flatScale;
    private Vector3 _targetScale;

    public override void Initialize()
    {
        _spriteTransform = Machine.transform.Find("sprite");
        _spriteRenderer = _spriteTransform.GetComponent<SpriteRenderer>();
        _mobStatusModule = Machine.GetModule<MobStatusModule>();

        _targetScale = _spriteTransform.localScale;
        _originalScale = _spriteTransform.localScale;
        _flatScale = new Vector3(0, _originalScale.y, 1);
    }

    public override void Update()
    {
        if (_spriteRenderer.isVisible)
        {
            UpdateDirection();
            HandleBounceAndTrail();
            HandleFlipCheck();
            HandleScaling();
        } 
    } 

    void UpdateDirection()
    {
        Vector2 rawDirection = new Vector2(_mobStatusModule.Direction.x, _mobStatusModule.Direction.z);
        rawDirection.Normalize();

        if (rawDirection != Vector2.zero)
        {
            rawDirection.x = Mathf.Abs(rawDirection.x) < 0.1f ? 0 : Mathf.Sign(rawDirection.x);
            rawDirection.y = Mathf.Abs(rawDirection.y) < 0.1f ? 0 : Mathf.Sign(rawDirection.y);
            rawDirection = ViewPort.GetRelativeDirection(rawDirection);

            _animDirection = new Vector2Int((int)rawDirection.x, (int)rawDirection.y);
        }
    }

    void HandleBounceAndTrail()
    {
        bool isMoving = _mobStatusModule.Direction != Vector3.zero;

        if (isMoving)
        {
            float newY = Mathf.PingPong(Time.time * BounceSpeed, BounceRange);
            _spriteTransform.localPosition = new Vector3(_spriteTransform.localPosition.x, newY, _spriteTransform.localPosition.z);

            if (Time.time >= _nextTrailTimer)
            {
                SmokeParticleHandler.CreateSmokeParticle(Machine.transform.position, false);
                _nextTrailTimer = Time.time + TrailFrequency;
                // AudioSystem.PlaySFX(Resources.Load<AudioClip>($"audio/sfx/footstep/footstep{Random.Range(1, 3)}"), 0.3f);
            }
        }
        else
        {
            _spriteTransform.localPosition = new Vector3(_spriteTransform.localPosition.x, 0, _spriteTransform.localPosition.z);
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
    }

    void HandleScaling()
    {
        if (_currentScaleState == 1)
        {
            _flipTimer += Time.deltaTime;
            _spriteTransform.localScale = Vector3.Lerp(_originalScale, _flatScale, _flipTimer / FlipDuration);

            if (_flipTimer >= FlipDuration)
            {
                _spriteTransform.localScale = _flatScale;
                _flipTimer = 0f;
                _currentScaleState = 2;
            }
        }
        else if (_currentScaleState == 2)
        {
            _flipTimer += Time.deltaTime;
            _spriteTransform.localScale = Vector3.Lerp(_flatScale, _targetScale, _flipTimer / FlipDuration);

            if (_flipTimer >= FlipDuration)
            {
                _spriteTransform.localScale = _targetScale;
                _currentScaleState = 0;
            }
        }
    }
}
