using System.Collections;
using UnityEngine;

public class GroundAnimationModule : MobModule
{
    private const float BounceSpeed = 1.65f;
    private const float BounceRange = 0.15f;
    // private const float BounceSpeed = 1f;
    // private const float BounceRange = 0.12f;
    private const float TrailFrequency = 0.5f;
    private const float FlipDuration = 0.06f; 
    
    private int _flipDirection;
    private float _nextTrailTimer = 0f;
    private Vector2Int _animDirection = Vector2Int.zero;

    private int _currentScaleState;
    private float _flipTimer;
    private Vector3 _originalScale;
    private Vector3 _flatScale;
    private Vector3 _targetScale;

    public override void Initialize()
    {
        _targetScale = Info.Sprite.localScale;
        _originalScale = Info.Sprite.localScale;
        _flatScale = new Vector3(0, _originalScale.y, 1); 
    }

 
    public override void Update()
    {
        if (Info.SpriteCharRenderer.isVisible)
        {
            if (Info.FaceTarget)
            {
                _animDirection = new Vector2Int((int)Mathf.Sign(Info.TargetScreenDir.x), 0);

                if (Info.Equipment != null)
                    EquipTrackTarget();
            }
            else
            { 
                SetDirectionToMovement();
            } 
            HandleBounceAndTrail();
            HandleFlipCheck();
            HandleScaling();
        } 
    }
 
    public void EquipTrackTarget()
    {
        float angle = Mathf.Atan2(Info.TargetScreenDir.y, Info.TargetScreenDir.x) * Mathf.Rad2Deg;

        if (angle > 90)
            angle = 180 - angle;
        else if (angle < -90)
            angle = -angle - 180;

        float z = Mathf.Lerp(-0.45f, 0.45f, (angle + 90) / 180);
        if (z is > 0f and <= 0.12f)
            z = 0.12f;
        else if (z is < 0f and >= -0.11f)
            z = -0.11f;
        // float angleX = (Mathf.Lerp(0, 90, Math.Abs(angle) / 45) + 360) % 360;
        // Normalize angle to 0–360
        Info.SpriteToolTrack.localPosition = new Vector3(0, 0.3f, z);
        Info.SpriteToolTrack.localRotation = Quaternion.Euler(80, 0, (angle + 360) % 360);
    }
    
    void SetDirectionToMovement()
    {
        Vector2 rawDirection = new Vector2(Info.Direction.x, Info.Direction.z);
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
        bool isMoving = Info.SpeedCurrent > 0.35 && Info.IsGrounded;

        if (isMoving)
        {
            float newY = Mathf.PingPong(Time.time * BounceSpeed, BounceRange);
            Info.Sprite.localPosition = new Vector3(Info.Sprite.localPosition.x, newY, Info.Sprite.localPosition.z);

            if (Time.time >= _nextTrailTimer)
            {
                SmokeParticleHandler.CreateSmokeParticle(Machine.transform.position, Info.IsPlayer);
                _nextTrailTimer = Time.time + TrailFrequency;
                if (Info.IsPlayer) Audio.PlaySFX($"footstep_{Random.Range(1, 3)}", 0.3f);
            }
        }
        else
        {
            Info.Sprite.localPosition = new Vector3(Info.Sprite.localPosition.x, 0, Info.Sprite.localPosition.z);
        }
    }

    void HandleFlipCheck()
    {
        if (_animDirection.x != 0)
        {
            if (!Mathf.Approximately(Mathf.Sign(_animDirection.x), Mathf.Sign(_targetScale.x)))
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
            Info.Sprite.localScale = Vector3.Lerp(_originalScale, _flatScale, _flipTimer / FlipDuration);

            if (_flipTimer >= FlipDuration)
            {
                Info.Sprite.localScale = _flatScale;
                _flipTimer = 0f;
                _currentScaleState = 2;
            }
        }
        else if (_currentScaleState == 2)
        {
            _flipTimer += Time.deltaTime;
            Info.Sprite.localScale = Vector3.Lerp(_flatScale, _targetScale, _flipTimer / FlipDuration);

            if (_flipTimer >= FlipDuration)
            {
                Info.Sprite.localScale = _targetScale;
                _currentScaleState = 0;
            }
        }
    }
}
