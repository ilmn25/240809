using UnityEngine;

public class GroundMovementModule : MovementModule
{  
    private const int UnstickRadius = 8;   // how far to look for an open cell
    private const float UnstickSpeed = 6f; // blocks/sec while climbing out of solid rock

    public GroundMovementModule() { updateMode = UpdateMode.Everyone; }

    private float _speedTarget;
    private float _speedAdjust; 
    private Vector3 _previousPosition;
    private Vector3 _directionBuffer = Vector3.zero;
    private Vector3 _abstractPos;
    public override void Initialize()
    {
        _abstractPos = Info.position;
    }

    /// <summary>If the entity is buried in solid blocks (spawned inside terrain,
    /// or a door/slab closed around it) every collision step is blocked, so walk
    /// it straight out to the nearest open cell. Returns true while escaping.</summary>
    private bool TryEscapeSolid()
    {
        Vector3 position = Machine.transform.position;
        if (!NavMap.IsBlocked(position)) return false;
        if (!NavMap.TryFindOpenSpot(Vector3Int.FloorToInt(position), UnstickRadius, out Vector3Int cell))
            return false;

        Info.Velocity = Vector3.zero;
        Machine.transform.position = Vector3.MoveTowards(position,
            new Vector3(cell.x + 0.5f, cell.y, cell.z + 0.5f), UnstickSpeed * DeltaTime);
        Info.position = Machine.transform.position;
        return true;
    }

    public override void Update()
    { 
        if (Info.Health <= 0) return;

        // Host runs physics for all entities; client only for owned entities
        if (!Helper.IsHost() && !Info.IsOwner()) return;

        if (Machine.transform.position.y < -1) Machine.transform.position = Helper.AddToVector(Machine.transform.position, 0, 100, 0);
        
        DeltaTime = Helper.GetDeltaTime();

        // Spawned/pushed inside solid blocks: collision would freeze every step,
        // so walk to the nearest open cell first.
        if (TryEscapeSolid()) { _abstractPos = Machine.transform.position; return; }

        if (!Info.IsInRenderRange) {
            _abstractPos += Info.Direction * (DeltaTime * Info.SpeedLogic);
            if (Vector3.Distance(Info.TargetPointPosition, _abstractPos) < 0.2f)
                Machine.transform.position = Info.TargetPointPosition;
            Info.Velocity = Vector3.zero;
            Info.position = Machine.transform.position;
            return; 
        }
        NewPosition = Machine.transform.position;

        HandleJump(); 
        if (Info.Direction != Vector3.zero)
        {  
            //! speeding up to start
            Info.SpeedCurrent = Mathf.Lerp(Info.SpeedCurrent, Info.SpeedTarget, DeltaTime / Info.AccelerationTime); 
            if (Info.Direction.x != 0 && Info.Direction.z != 0)
            {
                _speedAdjust = 1 / Mathf.Sqrt(2); // This is equivalent to 1 / 1.41421
            } else _speedAdjust = 1;
            NewPosition.x += Info.Direction.x * Info.SpeedCurrent * DeltaTime * _speedAdjust;
            NewPosition.z += Info.Direction.z * Info.SpeedCurrent * DeltaTime * _speedAdjust;
            
            if (IsMovable(Machine.transform.position) && !IsMovable(NewPosition))
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
        Info.position = Machine.transform.position;
    } 

    protected virtual void HandleJump()
    { 
        if ((Info.IsGrounded || Info.CanFly) && Info.Direction.y > 0)
        {
            Info.Velocity.y = Info.JumpVelocity; 
            Info.IsGrounded = false;
        }
    } 
} 

