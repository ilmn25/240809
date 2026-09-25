using UnityEngine;

public class BabySlimeMachine : SlimeMachine
{
    public static new Info CreateInfo()
    {
        return new EnemyInfo()
        {
            HealthMax = 3,
            Defense = 0,
            DistAttack = 2,
            DistAlert = 64,
            DistDisengage = 96,
            PathJump = 3,
            PathAir = 6,
            DistRoam = 12,
            DistStrafe = 3,
            SpeedGround = 0,
            SpeedLogic = 2,
            SpeedAir = 12,
            JumpVelocity = 12,
            DecelerationTime = 0,
            AccelerationTime = 0.1f,
            NormalSkipAmount = 3,
            mustLandFirst = true,
            MaxStuckCount = 700,
            PointLostDistance = 7,
        };
    }
}
