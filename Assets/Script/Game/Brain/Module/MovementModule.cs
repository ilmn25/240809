using System;
using UnityEngine;

[Serializable]
public abstract class MovementModule : DynamicModule
{
    private const float SlideDegree = 0.3f;
    
    protected float DeltaTime;  
    protected Vector3 NewPosition;
    
    private float _testPosition;
    private Vector3 _testPositionA;
    private Vector3 _testPositionB;
    private Vector3 _tempPosition;
 
    protected void HandleObstacle(Vector3 position)
    {
        NewPosition = Machine.transform.position;
        //! go any possible direction when going diagonally against a wall
        if (Info.Direction.x != 0 && IsMovable(new Vector3(position.x, Machine.transform.position.y, Machine.transform.position.z)))
        {
            NewPosition = new Vector3(position.x, Machine.transform.position.y, Machine.transform.position.z);
        }
        else if (Info.Direction.z != 0 && IsMovable(new Vector3(Machine.transform.position.x, Machine.transform.position.y, position.z)))
        {
            NewPosition = new Vector3(Machine.transform.position.x, Machine.transform.position.y, position.z);
        }
        else
        {
            //! slide against wall if possible
            if (Info.Direction.x != 0)
            {
                _testPosition = Machine.transform.position.x + SlideDegree * Info.Direction.x * Info.SpeedCurrent * DeltaTime;
                _testPositionA = new Vector3(_testPosition, Machine.transform.position.y, Machine.transform.position.z);
                _testPositionB = new Vector3(_testPosition, Machine.transform.position.y, Machine.transform.position.z);
                _testPositionA.z += -1 * Info.SpeedCurrent * DeltaTime;
                _testPositionB.z += 1 * Info.SpeedCurrent * DeltaTime;
                if (IsMovable(_testPositionA) && !IsMovable(_testPositionB))
                {
                    NewPosition = _testPositionA;
                }
                else if (!IsMovable(_testPositionA) && IsMovable(_testPositionB))
                {
                    NewPosition = _testPositionB;
                }
            }
            else
            {
                _testPosition = Machine.transform.position.z + SlideDegree * Info.Direction.z * Info.SpeedCurrent * DeltaTime;
                _testPositionA = new Vector3(Machine.transform.position.x, Machine.transform.position.y, _testPosition);
                _testPositionB = new Vector3(Machine.transform.position.x, Machine.transform.position.y, _testPosition);
                _testPositionA.x += -1 * Info.SpeedCurrent * DeltaTime;
                _testPositionB.x += 1 * Info.SpeedCurrent * DeltaTime;
                if (IsMovable(_testPositionA) && !IsMovable(_testPositionB))
                {
                    NewPosition = _testPositionA;
                }
                else if (!IsMovable(_testPositionA) && IsMovable(_testPositionB))
                {
                    NewPosition = _testPositionB;
                }
            }
        }
    } 
    
    protected void HandleMove()
    { 
        if (Main.BuildMode && Info != Main.PlayerInfo) NewPosition = Machine.transform.position;
        
        if (Info.Velocity.y > Info.Gravity) //terminal velocity
        {
            Info.Velocity.y += Info.Gravity * DeltaTime;
        } 
        
        _tempPosition = new Vector3(NewPosition.x + Info.Velocity.x * DeltaTime, NewPosition.y, NewPosition.z);
        if (!IsMovable(_tempPosition))
            Info.Velocity.x = 0; 
        else
            NewPosition = _tempPosition;
        
        _tempPosition = new Vector3(NewPosition.x, NewPosition.y, NewPosition.z + Info.Velocity.z * DeltaTime);
        if (!IsMovable(_tempPosition))
            Info.Velocity.z = 0; 
        else
            NewPosition = _tempPosition;
        
        Info.Velocity.x = Mathf.MoveTowards(Info.Velocity.x, 0f, 30 * Time.deltaTime);
        Info.Velocity.z = Mathf.MoveTowards(Info.Velocity.z, 0f, 30 * Time.deltaTime);
        
        _tempPosition = new Vector3(NewPosition.x, NewPosition.y + Info.Velocity.y * DeltaTime, NewPosition.z);
        if (!IsMovable(_tempPosition))
        { 
            if (Info.Velocity.y < 0) Info.IsGrounded = true;
            Info.Velocity.y = 0;
            Machine.transform.position = NewPosition;
        } 
        else
        {
            Machine.transform.position = _tempPosition;
        } 
        
        
    }
    
    protected bool IsMovable(Vector3 newPosition)
    {
        // Compute the exact block range the collider box overlaps.
        // Box half-extents: (0.35, 0.35, 0.25) centered at newPosition + (0, 0.35, 0).
        // Using precise extents avoids the 1-block padding that a fixed ±1 footprint would create.
        int minX = Mathf.FloorToInt(newPosition.x - 0.35f);
        int maxX = Mathf.FloorToInt(newPosition.x + 0.35f);
        int minZ = Mathf.FloorToInt(newPosition.z - 0.25f);
        int maxZ = Mathf.FloorToInt(newPosition.z + 0.25f);
        int minY = Mathf.FloorToInt(newPosition.y);
        int maxY = Mathf.FloorToInt(newPosition.y + 0.7f);

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                for (int z = minZ; z <= maxZ; z++)
                {
                    Vector3Int b = new Vector3Int(x, y, z);
                    if (World.IsInWorldBounds(b) && !NavMap.Get(b))
                        return false;
                }
            }
        }
        return true;
    } 
}