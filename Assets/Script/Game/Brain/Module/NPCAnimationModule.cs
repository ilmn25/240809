using System.Collections;
using UnityEngine;

public class NPCAnimationModule : Module
{
    private GameObject _sprite;
    private NPCMovementModule _npcMovementModule;
    private int _flipDirection;
    private float _nextTrailTimer = 0f;
    private Vector2Int _animDirection = Vector2Int.zero;

    private const float BOUNCE_SPEED = 1f;
    private const float BOUNCE_RANGE = 0.12f;
    private const float TRAIL_FREQUENCY = 0.5f;
    private const float FLIP_DURATION = 0.06f;

    private int _currentScaleState = 0;
    private float _flipTimer = 0f;
    private Vector3 _originalScale;
    private Vector3 _flatScale;
    private Vector3 _targetScale;

    public override void Initialize()
    {
        _sprite = Machine.transform.Find("sprite").gameObject;
        _npcMovementModule = Machine.GetModule<NPCMovementModule>();

        _targetScale = _sprite.transform.localScale;
        _originalScale = _sprite.transform.localScale;
        _flatScale = new Vector3(0, _originalScale.y, 1);
    }

    public void HandleAnimationUpdate()
    {
        UpdateDirection();
        HandleBounceAndTrail();
        HandleFlipCheck();
        HandleScaling();
    }

    void UpdateDirection()
    {
        Vector2 rawDirection = new Vector2(_npcMovementModule.GetDirection().x, _npcMovementModule.GetDirection().z);
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
        bool isMoving = _npcMovementModule.GetSpeed() > 0.35f && _npcMovementModule.IsGrounded();

        if (isMoving)
        {
            float newY = Mathf.PingPong(Time.time * BOUNCE_SPEED, BOUNCE_RANGE);
            _sprite.transform.localPosition = new Vector3(_sprite.transform.localPosition.x, newY, _sprite.transform.localPosition.z);

            if (Time.time >= _nextTrailTimer)
            {
                SmokeParticleHandler.CreateSmokeParticle(Machine.transform.position, false);
                _nextTrailTimer = Time.time + TRAIL_FREQUENCY;
                // AudioSystem.PlaySFX(Resources.Load<AudioClip>($"audio/sfx/footstep/footstep{Random.Range(1, 3)}"), 0.3f);
            }
        }
        else
        {
            _sprite.transform.localPosition = new Vector3(_sprite.transform.localPosition.x, 0, _sprite.transform.localPosition.z);
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
