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
    
    protected const float PointReachDistance = 0.45f; 
    protected const float RepathInterval = 0.5f;
    protected const int JumpSkipAmount = 1; 
    protected const float SelfMovedDistance = 0.002f;

    private bool _repathRoutine; 
    private bool _isPathFinding; 
    
    protected List<Node> Path;
    private List<Node> _pathQueued; 
    protected int NextPoint;
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

            if (Helper.SquaredDistance(Machine.transform.position, Path[^1].Position) < 1)
            {
                Info.PathingStatus = PathingStatus.Reached;  
                return true;
            } 
            return false;
        }

        
        if (Helper.SquaredDistance(Machine.transform.position, Info.Target.position) < 1)
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
        
        if (_nextPointQueued != -1)
        { 
            Path = _pathQueued;
            NextPoint = _nextPointQueued;
            _nextPointQueued = -1;
            _targetPositionPrevious = GetTargetPosition(); 
        }
        
        if (!IsTargetReached() && Path != null && NextPoint < Path.Count)
        {
            if (!Info.IsInRenderRange)
            {
                Info.TargetPointPosition = Path[NextPoint].Position + Vector3.up * 0.1f;
                Info.Direction = (Info.TargetPointPosition - Machine.transform.position).normalized; 
                if ( NextPoint < Path.Count -1 && Machine.transform.position == Info.TargetPointPosition) NextPoint++; 
            }
            else
               NextPoint = HandleMovePoint(NextPoint, Path); 
            
        } 
    }
    
    private int HandleMovePoint(int nextPoint, List<Node> path)
    { 
        if (nextPoint != path.Count - 1)
        {
            if (Info.MustLandFirst)
            {
                if (!Info.IsGrounded)
                {
                    Info.Direction = (path[nextPoint].Position - Machine.transform.position).normalized;
                    return nextPoint;
                };   
            }
            if (nextPoint == 0 ||
                 (((!path[nextPoint].IsFloat && 
                    math.distance(path[nextPoint].Position.y, Machine.transform.position.y) < 0.04f)
                   || path[nextPoint].IsFloat) &&
                  Vector2.Distance(
                      new Vector2(path[nextPoint].Position.x, path[nextPoint].Position.z), 
                      new Vector2(Machine.transform.position.x, Machine.transform.position.z)
                  ) < PointReachDistance/2))
            { 
                for (int i = 0; i < Info.NormalSkipAmount; i++)
                {
                    if (nextPoint >= path.Count - 1) return nextPoint;
                    nextPoint++;
                    if (path[nextPoint].IsFloat) break;
                }

                nextPoint = HandleAirMovePoint(nextPoint, path); 
            }  
            Info.Direction = (path[nextPoint].Position - Machine.transform.position).normalized;
        }
        else Info.Direction = Vector3.zero;
        return nextPoint;
    }

    private int HandleAirMovePoint(int nextPoint, List<Node> path)
    {
        if (nextPoint < path.Count - 1 && path[nextPoint].IsFloat &&
            path[nextPoint].Position.y > (int)Machine.transform.position.y - 1 &&
            (path[nextPoint + 1].Direction.x == path[nextPoint].Direction.x ||
             path[nextPoint + 1].Direction.z == path[nextPoint].Direction.z))
        {
            int initialPoint = nextPoint;
            while (nextPoint < path.Count - 1 && 
                   // nextPoint - initialPoint < Info.PointLostDistance - JumpSkipAmount &&
                   path[nextPoint].IsFloat && 
                   path[nextPoint].Position.y >= (int)Machine.transform.position.y &&
                   (path[nextPoint + 1].Direction.x == path[initialPoint].Direction.x ||
                    path[nextPoint + 1].Direction.z == path[initialPoint].Direction.z))
            {
                nextPoint++;
                if (Info.CanFly) break;
            }

            int potentialSkipPoint = nextPoint + JumpSkipAmount;
            if (potentialSkipPoint < path.Count - 1
                && !path[potentialSkipPoint].IsFloat
                && Mathf.Approximately(path[potentialSkipPoint].Position.y, path[nextPoint].Position.y))
            {
                nextPoint = potentialSkipPoint;
            }
        }
        return nextPoint;
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
        if (Path != null && Vector3.Distance(Machine.transform.position, Path[NextPoint].Position) > Info.PointLostDistance)
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
            nearestPoint = HandleAirMovePoint(nearestPoint, _pathQueued);
        }
        return nearestPoint; ;
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
            Gizmos.DrawSphere(Path[NextPoint].Position, 0.2f); // Adjust the radius as needed
        } 
    }
}
