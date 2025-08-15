// using System.Collections;
// using UnityEngine;
// using UnityEngine.UIElements;
//
// public class PlayerAnimationModule : Module
// { 
//     private const float BounceSpeed = 1.65f;
//     private const float BounceRange = 0.15f;
//     private const float TrailFrequency = 0.15f;
//     public const float FlipDuration = 0.05f;
//
//     private PlayerMovementModule PlayerMovementModule => GetModule<PlayerMovementModule>();
//     private PlayerInfo Info => (PlayerInfo) Machine.Info;
//     private int _flipDirection;
//     private float _nextTrailTimer = 0f;
//     private int _currentScaleState = 0;
//     private float _flipTimer = 0f;
//     private Vector3 _originalScale;
//     private Vector3 _flatScale;
//     private Vector3 _targetScale; 
//     private Vector2Int _animDirection = Vector2Int.zero;
//
//     public override void Initialize()
//     {
//         
//         _targetScale = Info.Sprite.localScale; 
//         _originalScale = Info.Sprite.localScale;
//         _flatScale = new Vector3(0, _originalScale.y, 1);
//
//         ViewPort.UpdateOrbitRotate += UpdateOrbit;
//     }
//
//     void UpdateOrbit()
//     { 
//         Machine.transform.rotation = ViewPort.CurrentRotation;
//     }
//
//     public override void Update()
//     {
//         if (Info.PlayerStatus != PlayerStatus.Active) return;
//         UpdateDirection();
//         HandleBounceAndTrail();
//         HandleFlipCheck();
//         HandleScaling();
//     } 
//
//     void UpdateDirection()
//     {
//         if (Inventory.CurrentItemData != null)
//         {
//             _animDirection = new Vector2Int((int)Mathf.Sign(Info.TargetScreenDir.x), 0);
//             TrackMouse();
//         }
//         else
//         {
//             if (PlayerMovementModule.RawInput != Vector2.zero)
//             {
//                 _animDirection = new Vector2Int(
//                     Mathf.RoundToInt(PlayerMovementModule.RawInput.x),
//                     Mathf.RoundToInt(PlayerMovementModule.RawInput.y)
//                 );
//             } 
//         }
//     }
//
//     private void TrackMouse()
//     {
//         float angle = Mathf.Atan2(Info.TargetScreenDir.y, 
//             Info.TargetScreenDir.x) * Mathf.Rad2Deg;
//
//         if (angle > 90)
//             angle = 180 - angle;
//         else if (angle < -90)
//             angle = -angle - 180;
//
//         float z = Mathf.Lerp(-0.45f, 0.45f, (angle + 90) / 180);
//         if (z is > 0f and <= 0.12f)
//             z = 0.12f;
//         else if (z is < 0f and >= -0.11f)
//             z = -0.11f; 
//         Info.SpriteToolTrack.localPosition = new Vector3(0, 0.3f, z);
//         Info.SpriteToolTrack.localRotation = Quaternion.Euler(80, 0, (angle + 360) % 360);
//     }
//     
//     void HandleBounceAndTrail()
//     {
//         bool isMoving = PlayerMovementModule.SpeedCurrent > 0.35 && Info.IsGrounded;
//
//         if (isMoving)
//         {
//             float newY = Mathf.PingPong(Time.time * BounceSpeed, BounceRange);
//             Info.Sprite.localPosition = new Vector3(Info.Sprite.localPosition.x, newY, Info.Sprite.localPosition.z);
//
//             if (Time.time >= _nextTrailTimer)
//             {
//                 SmokeParticleHandler.CreateSmokeParticle(Machine.transform.position, true);
//                 _nextTrailTimer = Time.time + TrailFrequency;
//                 Audio.PlaySFX($"footstep_{Random.Range(1, 3)}", 0.3f);
//             }
//         }
//         else
//         {
//             Info.Sprite.localPosition = new Vector3(Info.Sprite.localPosition.x, 0, Info.Sprite.localPosition.z);
//         }
//     }
//
//     void HandleFlipCheck()
//     {
//         if (_animDirection.x != 0)
//         {
//             if (Mathf.Sign(_animDirection.x) != Mathf.Sign(_targetScale.x))
//             {
//                 _flipDirection = _animDirection.x;
//                 _targetScale = new Vector3(Mathf.Sign(_flipDirection), _originalScale.y, _originalScale.z);
//                 _flipTimer = 0f;
//                 _currentScaleState = 1;
//             }
//         }
//     }
//
//     void HandleScaling()
//     {
//         if (_currentScaleState == 1)
//         {
//             _flipTimer += Time.deltaTime;
//             Info.Sprite.localScale = Vector3.Lerp(_originalScale, _flatScale, _flipTimer / FlipDuration);
//
//             if (_flipTimer >= FlipDuration)
//             {
//                 Info.Sprite.localScale = _flatScale;
//                 _flipTimer = 0f;
//                 _currentScaleState = 2;
//             }
//         }
//         else if (_currentScaleState == 2)
//         {
//             _flipTimer += Time.deltaTime;
//             Info.Sprite.localScale = Vector3.Lerp(_flatScale, _targetScale, _flipTimer / FlipDuration);
//
//             if (_flipTimer >= FlipDuration)
//             {
//                 Info.Sprite.localScale = _targetScale;
//                 _currentScaleState = 0;
//             }
//         }
//     }
// }
