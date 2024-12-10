using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public abstract class NPCPathFindAbstract : MonoBehaviour
{
    // parameters
    private Transform _target;
    private Boolean _isGrounded;
    
    // const
    private float _targetReachedInner;
    private float _targetReachedOuter;
    private float _pointReachDistance;
    private float _pointMissDistance; 
    private float _repathInterval; 
    private int _jumpSkipAmount;
    private int _scanCount;
    
    public NPCPathFindAbstract(
        float targetReachedInner = 1f, 
        float targetReachedOuter = 2f, 
        float pointReachDistance = 0.45f, 
        float pointMissDistance = 3f, 
        float repathInterval = 0.1f, 
        int jumpSkipAmount = 1,
        int scanCount = 3000)
    {
        _targetReachedInner = targetReachedInner;
        _targetReachedOuter = targetReachedOuter;
        _pointReachDistance = pointReachDistance;
        _pointMissDistance = pointMissDistance;
        _repathInterval = repathInterval;
        _scanCount = scanCount;
        _jumpSkipAmount = jumpSkipAmount;
    }

    public virtual bool IsValidPosition(Vector3Int pos, Vector3Int dir, Node currentNode)
    { 
        return false;
    } 
    
    public virtual bool GetTargetPosition()
    { 
        return false;
    } 
    public void SetTarget(Transform target)
    {
        _target = target;
    } 
     
      
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    // variables
    private bool _repathRoutine = false; 
    private bool _isPathFinding = false; 
    private bool _moveOccupied = false;
    private bool _targetMoved; 
    private bool _targetReached = false;
    
    private List<object[]> _path;
    private List<object[]> _pathQueued; 
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

    public async void HandlePathFindPassive(float passiveJumpSpeed)
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
                } else GetPath();
            } 
            else if (_path != null)
            {  
                await Task.Delay((int)(1500 / passiveJumpSpeed)); // Convert seconds to milliseconds
                if (this && _nextPoint < _path.Count -2)
                {
                    _nextPoint++;  
                    transform.position = Lib.AddToVector((Vector3)_path[_nextPoint][0], 0, 0.1f, 0);
                } else return;
            }   
            _moveOccupied = false; 
        }
    }

    public Vector3 HandlePathFindRandom(Boolean isGrounded)
    { 
        _isGrounded = isGrounded;
        _direction = Vector3.zero;

        if (!_repathRoutine) CheckRepathRoutineStill();
        if (_updateEntityPosition)
        { 
            _selfPositionPrevious = transform.position;
            _updateEntityPosition = false; 
        }
        
        if (_path == null || _nextPoint >= _path.Count - 2)
        {
            if (_nextPointQueued != -1)
            {
                _path = _pathQueued;
                _nextPoint = _nextPointQueued;
                _nextPointQueued = -1;
            }
            else
            {
                GetPath();
                return Vector3.zero;
            }
        } else if (_path != null)
        {
            HandleMovePoint(); 
        } 

        return _direction;
    }

    public Vector3 HandlePathFindActive(Boolean isGrounded)
    {
        
        _isGrounded = isGrounded;
        
        if (!_repathRoutine) CheckRepathRoutine();
 
        if (_updateEntityPosition)
        { 
            _selfPositionPrevious = transform.position;
            _updateEntityPosition = false; 
        }
        // if (_updateTargetPosition && PlayerMovementStatic.Instance._isGrounded) 
        if (_updateTargetPosition) 
        {
            _targetPositionPrevious = _target.transform.position;
            _updateTargetPosition = false;
        }
        
        _direction = Vector3.zero; 

        _targetDistance = Vector3.Distance(transform.position, _target.transform.position);
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
            _nextPointDistance = Vector3.Distance(transform.position,  (Vector3)_path[_nextPoint][0]);
            if (_isGrounded && _nextPointDistance < _pointReachDistance)
            {
                _nextPoint++;
            } 
            else if (_nextPoint < _path.Count - 1 && (bool)_path[_nextPoint][1] 
            && ((Vector3)_path[_nextPoint][0]).y >= (int)transform.position.y - 1)
            {
                while (_nextPoint < _path.Count - 1 && (bool)_path[_nextPoint][1]
                && ((Vector3)_path[_nextPoint][0]).y >= (int)transform.position.y)
                { 
                    _nextPoint++;
                }

                int potentialSkipPoint = _nextPoint + _jumpSkipAmount;
                if (potentialSkipPoint < _path.Count - 1 
                && !(bool)_path[potentialSkipPoint][1] 
                && Mathf.Approximately(((Vector3)_path[potentialSkipPoint][0]).y, ((Vector3)_path[_nextPoint][0]).y))
                {
                    _nextPoint = potentialSkipPoint;
                }
            } 
            else
            {
                if (_nextPoint == 0 ||
                    (((Vector3)_path[_nextPoint][0]).y > (int)transform.position.y &&
                    Vector2.Distance(
                        new Vector2(((Vector3)_path[_nextPoint][0]).x, ((Vector3)_path[_nextPoint][0]).z), 
                        new Vector2(transform.position.x, transform.position.z)
                    ) < _pointReachDistance))
                {
                    _nextPoint++;
                }
            } 
            _direction = ((Vector3)_path[_nextPoint][0] - transform.position).normalized; 
        } 
        else if (_target)
        { 
            _direction = (Lib.AddToVector(_target.transform.position, 0, -0.3f, 0) - transform.position).normalized;
        }
        else
        {
            Lib.Log();
            _direction = (Lib.AddToVector((Vector3)_path[^1][0], 0, -0.3f, 0) - transform.position).normalized;
        }
    }
     
 
    private async void CheckRepathRoutine()
    { 
        _repathRoutine = true;
        await Task.Delay((int)_repathInterval * 1000); 
 
        if (_nextPointQueued == -1 && !_targetReached && !_isPathFinding )
        {
            _targetMoved = Vector3.Distance(_targetPositionPrevious, _target.transform.position) > 0.8f;  //should be less than inner player near

            // if (PlayerMovementStatic.Instance._isGrounded && _targetMoved)
            if (_targetMoved)
            { 
                GetPath();  
                _updateTargetPosition = true;//! dont move
                // _playerPositionPrevious = _target.transform.position;
            }   
            else if (IsStuck()) 
            {
                GetPath();
            }
        }   
        _updateEntityPosition = true; 
        // _selfPositionPrevious = transform.position; 

        _repathRoutine = false;  
    }

    private async void CheckRepathRoutineStill()
    { 
        _repathRoutine = true;
        await Task.Delay((int)_repathInterval * 1000); 
 
        if (_nextPointQueued == -1 && IsStuck())
        {
            GetPath();
        }   
        _updateEntityPosition = true; 
        _repathRoutine = false;  
    }
    
    bool IsStuck()
    { 
        _selfMoveDistance = Vector2.Distance(
            new Vector2(_selfPositionPrevious.x, _selfPositionPrevious.z), 
            new Vector2(transform.position.x, transform.position.z)); 
        if (_selfMoveDistance < 0.001f)
        { 
            return true;
        }
        return false;
    }

    private async void GetPath()
    {
        if (_isPathFinding) return;
        _isPathFinding = true;
        try
        {  
             
            if (!_target)
                _pathQueued = await PathFindSingleton.Instance.FindPath(this, transform, null, 15);  
            else
                _pathQueued = await PathFindSingleton.Instance.FindPath(this, transform, _target, _scanCount); 
            
            if (this)
            {
                _positionWhenPathSearched = transform.position;
                await Task.Run(() => {
                    FindNearestPointEntity(ref _pathQueued);
                }); 
            }
        }
        catch (Exception ex)
        {
            Lib.Log("GetPath", ex);
            if (ex is not MissingReferenceException && ex is not NullReferenceException )
            {
                throw new Exception("An exception occurred in GetPath method.", ex);
            }
        }
    }
        
    private void FindNearestPointEntity(ref List<object[]> path)
    { 
        int nearestPoint;
        float distance, nearestDistance;
        if (path != null && path.Count > 0) 
        {
            nearestPoint = 0;
            nearestDistance = Vector3.Distance(_positionWhenPathSearched, (Vector3)path[0][0]);
            for (int i = 1; i < path.Count; i++)
            {
                distance = Vector3.Distance(_positionWhenPathSearched, (Vector3)path[i][0]);
                if (distance < nearestDistance)
                {
                    nearestPoint = i;
                    nearestDistance = distance;
                } else break; 
            } 
            if (nearestPoint == 0) nearestPoint = Mathf.Min(nearestPoint + 1, path.Count -1); 
            _nextPointQueued =  nearestPoint;
        }  
        _isPathFinding = false; 
    }

    private void OnDrawGizmos()
    {
        try
        {
            if (_path != null)
            {
                for (int i = 0; i < _path.Count - 1; i++)
                {
                    bool isTrue = (bool)_path[i][1];
                    Gizmos.color = isTrue ? Color.blue : Color.white;
                    Gizmos.DrawLine((Vector3)_path[i][0], (Vector3)_path[i + 1][0]);
                }
                Gizmos.color = Color.red;
                Gizmos.DrawSphere((Vector3)_path[_nextPoint][0], 0.2f); // Adjust the radius as needed
            }
        }
        catch (Exception )
        {
            return;
        }
    }
}
