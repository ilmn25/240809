using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class GroundMovementModule : MovementModule
{  
     
    private float _speedTarget;
    private float _speedAdjust; 
    private Vector3 _previousPosition;
    private Vector3 _directionBuffer = Vector3.zero;
    private Vector3 _abstractPos;
    
    public override void Update()
    { 
        if (Info.Health <= 0) return;
        if (Machine.transform.position.y < -1) Machine.transform.position = Helper.AddToVector(Machine.transform.position, 0, 100, 0);
        
        DeltaTime = Helper.GetDeltaTime();
        if (!Info.IsInRenderRange) {
            _abstractPos += Info.Direction * (DeltaTime * Info.SpeedLogic);
            if (Vector3.Distance(Info.TargetPointPosition, _abstractPos) < 0.2f)
                Machine.transform.position = Info.TargetPointPosition;
            Info.Velocity = Vector3.zero;
            return; 
        }
        NewPosition = Machine.transform.position;

        HandleJump(); 
        if (Info.Direction != Vector3.zero)
        {  
            //! speeding up to start
            Info.SpeedCurrent = Mathf.Lerp(Info.SpeedCurrent, Info.SpeedTarget, DeltaTime / Info.AccelerationTime);
            // if (Info.IsPlayer)Utility.Log(Info.SpeedTarget, Info.Direction);
            if (Info.Direction.x != 0 && Info.Direction.z != 0)
            {
                _speedAdjust = 1 / Mathf.Sqrt(2); // This is equivalent to 1 / 1.41421
            } else _speedAdjust = 1;
            NewPosition.x += Info.Direction.x * Info.SpeedCurrent * DeltaTime * _speedAdjust;
            NewPosition.z += Info.Direction.z * Info.SpeedCurrent * DeltaTime * _speedAdjust;

            if (!IsMovable(NewPosition))
            {
                HandleObstacle(NewPosition);
            } 
            _directionBuffer = Info.Direction;
        }
        else if (Info.SpeedCurrent != 0)
        {
            //! slowing down to stop
            Info.SpeedCurrent = (Info.SpeedCurrent < 0.05f) ? 0f : Mathf.Lerp(Info.SpeedCurrent, 0, DeltaTime / Info.DecelerationTime);

            NewPosition.x += _directionBuffer.x * Info.SpeedCurrent * DeltaTime;
            NewPosition.z += _directionBuffer.z * Info.SpeedCurrent * DeltaTime;

            if (!IsMovable(NewPosition))
            {
                Info.SpeedCurrent /= 2;
                NewPosition = Machine.transform.position;
            }
        }
 
        HandleMove();  
        _abstractPos = Machine.transform.position;
    } 

    protected virtual void HandleJump()
    { 
        // if (_isGrounded && _direction.y > 0 && _npcPathFindInst._nextPointDistance < 2f)
        if (Info.IsGrounded && Info.Direction.y > 0)
        {
            Info.Velocity.y = Info.JumpVelocity; 
            Info.IsGrounded = false;
        }
    } 
} 

