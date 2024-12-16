using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public abstract class PathingModule : Module
{
    // parameters
    protected Transform Target;
    protected Vector3 TargetPosition;
    private Boolean _isGrounded;
    
    // const
    private float _targetReachedInner;
    private float _targetReachedOuter;
    private float _pointReachDistance;
    private float _repathInterval; 
    private int _jumpSkipAmount;
    
    public PathingModule(
        float targetReachedInner = 1f, 
        float targetReachedOuter = 2f, 
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

    public virtual bool IsValidPosition(Vector3Int pos, Vector3Int dir, Node currentNode)
    { 
        return false;
    } 
    public virtual Vector3 GetTargetPosition()
    {
        return Vector3.zero;
    } 
    public virtual void OnStuck() { } 
    public void SetTarget(Transform target)
    {
        Target = target; 
    }
     
      
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    // variables
    private bool _repathRoutine = false; 
    private bool _isPathFinding = false; 
    private bool _moveOccupied = false;
    private bool _targetMoved; 
    private bool _targetReached = false;
    
    private List<Node> _path;
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
    
    private Vector3 _positionWhenPathSearched; 
    private Vector3 _direction; 

    public async void PassivePathFollow(float speed)
    {  
        if (!_moveOccupied)
        {
            _moveOccupied = true;  
            if (_path == null || _nextPoint >= _path.Count - 2)
            {
                if (_nextPointQueued != -1)
                {
                    _path = _pathQueued; 
                    _nextPoint = _nextPointQueued;
                    _nextPointQueued = -1;
                } else Repath();
            } 
            else if (_path != null)
            {  
                await Task.Delay((int)(1500 / speed)); // Convert seconds to milliseconds
                if (Machine.transform && _nextPoint < _path.Count -2)
                {
                    _nextPoint++;  
                    Machine.transform.position = Lib.AddToVector(_path[_nextPoint].Position, 0, 0.1f, 0);
                } else return;
            }   
            _moveOccupied = false; 
        }
    } 

    public Vector3 GetNextDirection(Boolean isGrounded)
    {
        
        _isGrounded = isGrounded;
        
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
            _path = null;
            _targetReached = _targetDistance < _targetReachedOuter;
        }
         
        if (_path != null 
        && !_targetReached 
        && _nextPoint < _path.Count)
        {
            HandleMovePoint(); 
        }
 
        if (_nextPointQueued != -1)
        {
            _path = _pathQueued;
            _nextPoint = _nextPointQueued;
            _nextPointQueued = -1;
        }
 

        return _direction;
    }
    
    private void HandleMovePoint()
    { 
        if (_nextPoint != _path.Count - 1)
        { 
            _nextPointDistance = Vector3.Distance(Machine.transform.position,  _path[_nextPoint].Position);
            if (_isGrounded && _nextPointDistance < _pointReachDistance)
            {
                _nextPoint++;
            } 
            else if (_nextPoint < _path.Count - 1 && _path[_nextPoint].IsFloat 
            && _path[_nextPoint].Position.y >= (int)Machine.transform.position.y - 1)
            {
                while (_nextPoint < _path.Count - 1 && _path[_nextPoint].IsFloat
                && _path[_nextPoint].Position.y >= (int)Machine.transform.position.y)
                { 
                    _nextPoint++;
                }

                int potentialSkipPoint = _nextPoint + _jumpSkipAmount;
                if (potentialSkipPoint < _path.Count - 1 
                && !_path[potentialSkipPoint].IsFloat 
                && Mathf.Approximately(_path[potentialSkipPoint].Position.y, _path[_nextPoint].Position.y))
                {
                    _nextPoint = potentialSkipPoint;
                }
            } 
            else
            {
                if (_nextPoint == 0 ||
                    (_path[_nextPoint].Position.y > (int)Machine.transform.position.y &&
                    Vector2.Distance(
                        new Vector2(_path[_nextPoint].Position.x, _path[_nextPoint].Position.z), 
                        new Vector2(Machine.transform.position.x, Machine.transform.position.z)
                    ) < _pointReachDistance))
                {
                    _nextPoint++;
                }
            } 
            _direction = (_path[_nextPoint].Position - Machine.transform.position).normalized; 
        } 
        else
        { 
            _direction = (Lib.AddToVector(GetTargetPosition(), 0, -0.3f, 0) - Machine.transform.position).normalized;
        } 
    }
     
 
    private async void CheckRepathRoutine()
    { 
        _repathRoutine = true;
        await Task.Delay((int)_repathInterval * 1000); 
 
        if (_nextPointQueued == -1 && !_targetReached && !_isPathFinding )
        {
            _targetMoved = Vector3.Distance(_targetPositionPrevious, GetTargetPosition()) > 0.8f;  //should be less than inner player near

            // if (PlayerMovementStatic.Instance._isGrounded && _targetMoved)
            if (_targetMoved)
            { 
                Repath();  
                _updateTargetPosition = true;//! dont move
                // _playerPositionPrevious = GetTargetPosition();
            }   
            else if (IsStuck())
            {
                OnStuck();
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
        return await PathFindSingleton.FindPath(this, 7000); 
    }
    
    private async void Repath()
    {
        if (_isPathFinding) return;
        _isPathFinding = true;
        try
        {  
            _pathQueued = await GetPath(); 
            if (Machine.transform)
            {
                _positionWhenPathSearched = Machine.transform.position;
                _nextPointQueued = FindNearestPointEntity(ref _pathQueued);
            }
        }
        catch (Exception ex)
        {
            if (ex is not MissingReferenceException && ex is not NullReferenceException )
                throw new Exception("An exception occurred in GetPath method.", ex);
        }
        _isPathFinding = false; 
    }
        
    private int FindNearestPointEntity(ref List<Node> path)
    { 
        int nearestPoint = 0;
        float distance, nearestDistance;
        if (path != null && path.Count > 0) 
        {
            nearestDistance = PathFindSingleton.SquaredDistance(Machine.transform.position, path[0].Position);
            for (int i = 1; i < path.Count; i++)
            {
                distance = PathFindSingleton.SquaredDistance(Machine.transform.position, path[i].Position);
                if (distance < nearestDistance)
                {
                    nearestPoint = i;
                    nearestDistance = distance;
                } else break; 
            } 
            if (nearestPoint == 0) nearestPoint = Mathf.Min(1, path.Count -1); 
        }
        return nearestPoint;
    }

    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    public void DrawGizmos()
    { 
        if (_path != null)
        {
            for (int i = 0; i < _path.Count - 1; i++)
            {
                Gizmos.color = _path[i].IsFloat ? Color.blue : Color.white;
                Gizmos.DrawLine(_path[i].Position, _path[i + 1].Position);
            }
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_path[_nextPoint].Position, 0.2f); // Adjust the radius as needed
        } 
    }
}
