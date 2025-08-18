using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

public enum PathingStatus {Pending, Reached, Stuck}
public enum PathingTarget {None, Target, Strafe, Evade, Escape, Roam}
public abstract class PathingModule : MobModule
{ 
    public PathingTarget PathingTarget = PathingTarget.None;  
    
    private const float PointReachDistance = 0.45f;
    private const float PointLostDistance = 5;
    private const float RepathInterval = 0.5f;
    private const int JumpSkipAmount = 1;
    private const float SelfMovedDistance = 0.002f;

    private bool _repathRoutine; 
    private bool _isPathFinding; 
    
    protected List<Node> Path;
    private List<Node> _pathQueued; 
    private int _nextPoint;
    private int _nextPointQueued = -1;
    
    private float _nextPointDistance; 
    private float _targetDistance;
    private float _selfMoveDistance;
    private Vector3 _targetPositionPrevious;
    private Vector3 _selfPositionPrevious; 
    
    private bool _updateTargetPosition;
    private bool _updateSelfPosition;

    private int _stuckCount;   
    protected int RepathCount;
 
 

    protected const int MaxRepathCount = 4;    
    public void SetTarget(PathingTarget pathingTarget)
    {
        PathingTarget = pathingTarget;
        Path = null;
        _nextPointQueued = -1;
        Info.PathingStatus = PathingStatus.Pending;
        Info.Direction = Vector3.zero;
        RepathCount = 0;
        _stuckCount = 0;
        Repath();
    }

    public abstract bool IsValidPosition(Vector3Int pos, Vector3Int dir, Node currentNode);
    public abstract void OnStuck(); 

    public bool IsTargetReached()
    {
        if (PathingTarget != PathingTarget.Target)
        {
            if (Path == null) return false;

            if (Vector3.Distance(Machine.transform.position, Path[^1].Position) < 1)
            {
                Info.PathingStatus = PathingStatus.Reached;  
                return true;
            } 
            return false;
        }

        
        if (Vector3.Distance(Machine.transform.position, Info.Target.position) < 1)
        {
            Info.PathingStatus = PathingStatus.Reached; 
            return true;
        } 
        return false;
    }
    
    public Vector3 GetTargetPosition()
    {
        if (PathingTarget == PathingTarget.Target)
            return Info.Target.position;
        
        return Path == null? Vector3.down : Path[^1].Position;
    }
     
    public override void Update()
    {
        if (Info.PathingStatus != PathingStatus.Pending || PathingTarget == PathingTarget.None) return;
        Info.Direction = Vector3.zero;       
        if (!_repathRoutine) CheckRepathRoutine();

        if (_updateSelfPosition)
        {
            _selfPositionPrevious = Machine.transform.position;
            _updateSelfPosition = false;
        }

        if (_updateTargetPosition)
        {
            _targetPositionPrevious = Info.Target.position;
            _updateTargetPosition = false;
        }

        
        if (!IsTargetReached() && Path != null && _nextPoint < Path.Count)
            HandleMovePoint();

        if (_nextPointQueued != -1)
        {
            Path = _pathQueued;
            _nextPoint = _nextPointQueued;
            _nextPointQueued = -1;
            _targetPositionPrevious = GetTargetPosition();
        }
    }
    
    private void HandleMovePoint()
    { 
        if (_nextPoint != Path.Count - 1)
        { 
            if (_nextPoint == 0 ||
                 (((!Path[_nextPoint].IsFloat 
                    && math.distance(Path[_nextPoint].Position.y, Machine.transform.position.y) < 0.04f)
                   || Path[_nextPoint].IsFloat) &&
                  Vector2.Distance(
                      new Vector2(Path[_nextPoint].Position.x, Path[_nextPoint].Position.z), 
                      new Vector2(Machine.transform.position.x, Machine.transform.position.z)
                  ) < PointReachDistance/2))
            {
                _nextPoint++; 
                if (_nextPoint < Path.Count - 1 && Path[_nextPoint].IsFloat 
                && Path[_nextPoint].Position.y > (int)Machine.transform.position.y - 1 &&
                (Path[_nextPoint + 1].Direction.x == Path[_nextPoint].Direction.x ||
                 Path[_nextPoint + 1].Direction.z == Path[_nextPoint].Direction.z))
                {
                    Node initialNode = Path[_nextPoint];
                    while (_nextPoint < Path.Count - 1 && Path[_nextPoint].IsFloat
                    && Path[_nextPoint].Position.y >= (int)Machine.transform.position.y &&
                    (Path[_nextPoint + 1].Direction.x == initialNode.Direction.x ||
                     Path[_nextPoint + 1].Direction.z == initialNode.Direction.z))
                    {  
                        _nextPoint++;
                    }

                    int potentialSkipPoint = _nextPoint + JumpSkipAmount;
                    if (potentialSkipPoint < Path.Count - 1 
                    && !Path[potentialSkipPoint].IsFloat 
                    && Mathf.Approximately(Path[potentialSkipPoint].Position.y, Path[_nextPoint].Position.y))
                    {
                        _nextPoint = potentialSkipPoint;
                    }
                } 
                // else
                // {
                //     if (_nextPoint == 0 ||
                //         (Path[_nextPoint].Position.y > (int)Machine.transform.position.y &&
                //         Vector2.Distance(
                //             new Vector2(Path[_nextPoint].Position.x, Path[_nextPoint].Position.z), 
                //             new Vector2(Machine.transform.position.x, Machine.transform.position.z)
                //         ) < PointReachDistance))
                //     {
                //         _nextPoint++;
                //     }
                // } 
            }  
                
            Info.Direction = (Path[_nextPoint].Position - Machine.transform.position).normalized; 
        } 
        else
        { 
            Info.Direction = (Helper.AddToVector(GetTargetPosition(), 0, -0.3f, 0) - Machine.transform.position).normalized;
        } 
    }
 
    private async void CheckRepathRoutine()
    { 
        _repathRoutine = true;
        await Task.Delay((int)RepathInterval * 1000); 
 
        if (_nextPointQueued == -1 && Info.PathingStatus == PathingStatus.Pending && !_isPathFinding )
        {
            if (PathingTarget == PathingTarget.Target &&
                Vector3.Distance(_targetPositionPrevious, Info.Target.position) > Info.DistAttack)
            {
                Repath();  
                _updateTargetPosition = true;//! dont move
            } 
            else if (IsStuck())
            {
                OnStuck();
            }
        }   
        _updateSelfPosition = true; 
        // _selfPositionPrevious = Machine.transform.position; 

        _repathRoutine = false;  
    }
  
    private bool IsStuck()
    {
        if (Path != null && Vector3.Distance(Machine.transform.position, Path[_nextPoint].Position) > PointLostDistance)
        {
            return true;
        }
        _selfMoveDistance = Vector2.Distance(
            new Vector2(_selfPositionPrevious.x, _selfPositionPrevious.z), 
            new Vector2(Machine.transform.position.x, Machine.transform.position.z)); 
        if (_selfMoveDistance < SelfMovedDistance)
        { 
            _stuckCount++;
            if (_stuckCount > Info.MaxStuckCount)
            { 
                _stuckCount = 0;
                return true;
            } 
            return false;
        }

        _stuckCount = 0;
        return false;
    }

    protected virtual async Task<List<Node>> GetPath()
    {
        return await PathFind.FindPath(this, 1); //placeholder, must be overwritten with logic
    }
    
    protected async void Repath()
    {
        if (_isPathFinding) return;
        _isPathFinding = true;
        try
        {  
            _pathQueued = await GetPath(); 
            if (Machine.transform) _nextPointQueued = GetNeartestPoint();
        }
        catch (Exception ex)
        {
            if (ex is not MissingReferenceException && ex is not NullReferenceException )
                throw new Exception("An exception occurred in GetPath method.", ex);
        }
        _isPathFinding = false; 
    }
        
    private int GetNeartestPoint()
    { 
        int nearestPoint = 0;
        float distance, nearestDistance;
        if (_pathQueued != null && _pathQueued.Count > 0) 
        {
            nearestDistance = Helper.SquaredDistance(Machine.transform.position, _pathQueued[0].Position);
            for (int i = 1; i < _pathQueued.Count; i++)
            {
                distance = Helper.SquaredDistance(Machine.transform.position, _pathQueued[i].Position);
                if (distance < nearestDistance)
                {
                    nearestPoint = i;
                    nearestDistance = distance;
                } else break; 
            } 
            if (nearestPoint == 0) nearestPoint = Mathf.Min(1, _pathQueued.Count -1); 
        }
        return nearestPoint;
    }

    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    public void DrawGizmos()
    { 
        if (Path != null)
        {
            for (int i = 0; i < Path.Count - 1; i++)
            {
                Gizmos.color = Path[i].IsFloat ? Color.blue : Color.white;
                Gizmos.DrawLine(Path[i].Position, Path[i + 1].Position);
            }
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(Path[_nextPoint].Position, 0.2f); // Adjust the radius as needed
        } 
    }
}
