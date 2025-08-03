using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public abstract class PathingModule : Module
{
    // parameters 
    private NPCMovementModule _npcMovementModule;
    protected Transform Target;
    public void SetTarget(Transform target)
    {
        Target = target; 
    }
    
    // const
    private float _targetReachedInner;
    private float _targetReachedOuter;
    private float _pointReachDistance;
    private float _repathInterval; 
    private int _jumpSkipAmount;
    
    public PathingModule(
        float targetReachedInner = 0.5f, 
        float targetReachedOuter = 1.5f, 
        float pointReachDistance = 0.45f, 
        float repathInterval = 0.1f, 
        int jumpSkipAmount = 1)
    { 
        _targetReachedInner = targetReachedInner;
        _targetReachedOuter = targetReachedOuter;
        _pointReachDistance = pointReachDistance;
        _repathInterval = repathInterval;
        _jumpSkipAmount = jumpSkipAmount;
    }

    public override void Initialize()
    {
        _npcMovementModule = Machine.GetModule<NPCMovementModule>();
    }

    public abstract bool IsValidPosition(Vector3Int pos, Vector3Int dir, Node currentNode);
    public abstract Vector3 GetTargetPosition(); 
     
      
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    // variables
    private bool _repathRoutine = false; 
    private bool _isPathFinding = false; 
    private bool _moveOccupied = false;
    private bool _targetMoved; 
    private bool _targetReached = false;
    
    protected List<Node> Path;
    private List<Node> _pathQueued; 
    private int _nextPoint = 0;
    private int _nextPointQueued = -1;
    
    private float _nextPointDistance; 
    private float _targetDistance;
    private float _selfMoveDistance;
    private Vector3 _targetPositionPrevious;
    private Vector3 _selfPositionPrevious; 
    
    private bool _updateTargetPosition = false;
    private bool _updateEntityPosition = false; 
    
    private Vector3 _direction; 

    public async void PassivePathFollow(float speed)
    {  
        if (!_moveOccupied)
        {
            _moveOccupied = true;  
            if (Path == null || _nextPoint >= Path.Count - 2)
            {
                if (_nextPointQueued != -1)
                {
                    Path = _pathQueued; 
                    _nextPoint = _nextPointQueued;
                    _nextPointQueued = -1;
                } else Repath();
            } 
            else if (Path != null)
            {  
                await Task.Delay((int)(1500 / speed)); // Convert seconds to milliseconds
                if (Machine && _nextPoint < Path.Count -2)
                {
                    _nextPoint++;  
                    Machine.transform.position = Utility.AddToVector(Path[_nextPoint].Position, 0, 0.1f, 0);
                } else return;
            }   
            _moveOccupied = false; 
        }
    } 

    public Vector3 GetNextDirection()
    { 
        
        if (!_repathRoutine) CheckRepathRoutine();
 
        if (_updateEntityPosition)
        { 
            _selfPositionPrevious = Machine.transform.position;
            _updateEntityPosition = false; 
        }

        if (_updateTargetPosition) 
        {
            _targetPositionPrevious = GetTargetPosition();
            _updateTargetPosition = false;
        }
        
        _direction = Vector3.zero; 

        _targetDistance = Vector3.Distance(Machine.transform.position, GetTargetPosition());
        if (!_targetReached)
        {   
            _targetReached = _targetDistance < _targetReachedInner;
        } else
        {
            Path = null;
            _targetReached = _targetDistance < _targetReachedOuter;
        }
         
        if (Path != null 
        && !_targetReached 
        && _nextPoint < Path.Count)
        {
            HandleMovePoint(); 
        }
 
        if (_nextPointQueued != -1)
        {
            Path = _pathQueued; 
            _nextPoint = _nextPointQueued;
            _nextPointQueued = -1;
            _targetPositionPrevious = GetTargetPosition();
        }

        return _direction;
    }
    
    private void HandleMovePoint()
    { 
        if (_nextPoint != Path.Count - 1)
        { 
            _nextPointDistance = Vector3.Distance(Machine.transform.position,  Path[_nextPoint].Position);
            if (_npcMovementModule.IsGrounded() && _nextPointDistance < _pointReachDistance)
            {
                _nextPoint++;
            } 
            else if (_nextPoint < Path.Count - 1 && Path[_nextPoint].IsFloat 
            && Path[_nextPoint].Position.y >= (int)Machine.transform.position.y - 1)
            {
                while (_nextPoint < Path.Count - 1 && Path[_nextPoint].IsFloat
                && Path[_nextPoint].Position.y >= (int)Machine.transform.position.y)
                { 
                    _nextPoint++;
                }

                int potentialSkipPoint = _nextPoint + _jumpSkipAmount;
                if (potentialSkipPoint < Path.Count - 1 
                && !Path[potentialSkipPoint].IsFloat 
                && Mathf.Approximately(Path[potentialSkipPoint].Position.y, Path[_nextPoint].Position.y))
                {
                    _nextPoint = potentialSkipPoint;
                }
            } 
            else
            {
                if (_nextPoint == 0 ||
                    (Path[_nextPoint].Position.y > (int)Machine.transform.position.y &&
                    Vector2.Distance(
                        new Vector2(Path[_nextPoint].Position.x, Path[_nextPoint].Position.z), 
                        new Vector2(Machine.transform.position.x, Machine.transform.position.z)
                    ) < _pointReachDistance))
                {
                    _nextPoint++;
                }
            } 
            _direction = (Path[_nextPoint].Position - Machine.transform.position).normalized; 
        } 
        else
        { 
            _direction = (Utility.AddToVector(GetTargetPosition(), 0, -0.3f, 0) - Machine.transform.position).normalized;
        } 
    }
     
 
    private async void CheckRepathRoutine()
    { 
        _repathRoutine = true;
        await Task.Delay((int)_repathInterval * 1000); 
 
        if (_nextPointQueued == -1 && !_targetReached && !_isPathFinding )
        {
            _targetMoved = Vector3.Distance(_targetPositionPrevious, GetTargetPosition()) > 2;  //should be less than inner player near

            // if (PlayerMovementStatic.Instance._isGrounded && _targetMoved)
            if (_targetMoved)
            { 
                Repath();  
                _updateTargetPosition = true;//! dont move
                // _playerPositionPrevious = GetTargetPosition();
            }   
            else if (_nextPoint != 1 && IsStuck())
            {
                Repath();
            }
        }   
        _updateEntityPosition = true; 
        // _selfPositionPrevious = Machine.transform.position; 

        _repathRoutine = false;  
    }
 
    
    private bool IsStuck()
    { 
        _selfMoveDistance = Vector2.Distance(
            new Vector2(_selfPositionPrevious.x, _selfPositionPrevious.z), 
            new Vector2(Machine.transform.position.x, Machine.transform.position.z)); 
        if (_selfMoveDistance < 0.001f)
        { 
            return true;
        }
        return false;
    }

    protected virtual async Task<List<Node>> GetPath()
    { 
        return await PathFind.FindPath(this, 7000); 
    }
    
    protected async void Repath()
    {
        if (_isPathFinding) return;
        _isPathFinding = true;
        try
        {  
            _pathQueued = await GetPath(); 
            if (Machine.transform) _nextPointQueued = FindNearestPointEntity();
        }
        catch (Exception ex)
        {
            if (ex is not MissingReferenceException && ex is not NullReferenceException )
                throw new Exception("An exception occurred in GetPath method.", ex);
        }
        _isPathFinding = false; 
    }
        
    private int FindNearestPointEntity()
    { 
        int nearestPoint = 0;
        float distance, nearestDistance;
        if (_pathQueued != null && _pathQueued.Count > 0) 
        {
            nearestDistance = Utility.SquaredDistance(Machine.transform.position, _pathQueued[0].Position);
            for (int i = 1; i < _pathQueued.Count; i++)
            {
                distance = Utility.SquaredDistance(Machine.transform.position, _pathQueued[i].Position);
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
