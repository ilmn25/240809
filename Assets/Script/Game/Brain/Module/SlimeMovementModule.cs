using UnityEngine;

public class SlimeMovementModule : GroundMovementModule
{
    private int _timer;
    
    protected override void HandleJump()
    {
        if (Info.IsGrounded) Info.SpeedCurrent = 0;
            
        if (!Info.IsGrounded || Info.Direction == Vector3.zero) return;
        _timer++;
        if (_timer > 100)
        {
            _timer = 0;
            Info.Velocity.y = Info.JumpVelocity; 
            Info.IsGrounded = false;
        }
    }
}