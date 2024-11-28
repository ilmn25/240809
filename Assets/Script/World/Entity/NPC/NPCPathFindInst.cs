using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Serialization;
using Object = System.Object;

public class NPCPathFindInst : MonoBehaviour
{
    private GameObject _target;
    private NPCMovementInst _npcMovementInst;
     
    public int[] AGENT = new int[5];
    private float _targetReachedInner = 1f;
    private float _targetReachedOuter = 2f;
    private float _pointReachDistance = 0.45f;
    private float _pointMissDistance = 2f; 
    private float _repathInterval = 0.1f; 
    private int _jumpSkipAmount = 1;
    
    private bool _repathRoutine = false; 
    private bool _isPathFinding = false; 
    private bool _moveOccupied = false;
    private bool _targetMoved; 
    private bool _targetReached = false;
    private List<object[]> _path;
    private List<object[]> _pathQueued; 
    private int _nextPoint = 0;
    private Vector3 _nextPointPosition;
    private int _nextPointQueued = -1;
    private float _nextPointDistance; 
    private float _targetDistance;
    private float _selfMoveDistance;
    private Vector3 _targetPositionPrevious;
    private Vector3 _selfPositionPrevious; 
    private bool _updateTargetPosition = false;
    private bool _updateEntityPosition = false; 
    private Vector3 _direction;
    private Vector3 _positionWhenPathSearched; 

    private void Awake()
    {
        _target = Game.Player;
        _npcMovementInst = GetComponent<NPCMovementInst>();
    }

    public void SetTarget(GameObject target)
    {
        _target = target;
    } 
    
    
    

    public async void HandlePathFindPassive()
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
                await Task.Delay((int)(1500 / _npcMovementInst.SPEED_WALK)); // Convert seconds to milliseconds
                if (this != null && _nextPoint < _path.Count -2)
                {
                    _nextPoint++;  
                    transform.position = Lib.AddToVector((Vector3)_path[_nextPoint][0], 0, 0.1f, 0);
                } else return;
            }   
            _moveOccupied = false; 
        }
    }

    public Vector3 HandlePathFindActive()
    {
        if (!_repathRoutine) RepathRoutine();
 
        if (_updateEntityPosition && _npcMovementInst.IsGrounded())
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

        void HandleMovePoint()
        { 
            if (_nextPoint != _path.Count - 1)
            { 
                _nextPointPosition = (Vector3)_path[_nextPoint][0];  
                _nextPointDistance = Vector3.Distance(transform.position, _nextPointPosition); 
                if (_npcMovementInst.IsGrounded() && _nextPointDistance < _pointReachDistance)
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
            } else _direction = (Lib.AddToVector(_target.transform.position, 0, -0.3f, 0) - transform.position).normalized;
            
             
        }

        return _direction;
    }

 















 





 
    private async void RepathRoutine()
    { 
        _repathRoutine = true;
        await Task.Delay((int)_repathInterval * 1000); 
 
        if (!_targetReached && !_isPathFinding)
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

    bool IsStuck()
    {
        if (_npcMovementInst.IsGrounded()) 
        { 
            if (_nextPointDistance > _pointMissDistance)
            {
                return true;
            }
            
            _selfMoveDistance = Vector2.Distance(
                new Vector2(_selfPositionPrevious.x, _selfPositionPrevious.z), 
                new Vector2(transform.position.x, transform.position.z)); 
            if (_selfMoveDistance < 0.001f)
            { 
                return true;
            }
        }
        return false;
    }


    private async void GetPath()
    {
        if (_isPathFinding) return;
        _isPathFinding = true;
        try
        {  
            // Vector3Int target = (_target == new Vector3Int()) ? new Vector3Int(0, -1, 0) : _target; 
            _pathQueued = await NPCPathFindStatic.Instance.FindPath(AGENT, transform, _target.transform);  
            if (this != null)
            {
                _positionWhenPathSearched = transform.position;
                await Task.Run(() => {
                    FindNearestPointEntity(ref _pathQueued);
                });
                // ThreadPool.QueueUserWorkItem(state => {
                //     System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.Highest;
                //     FindNearestPointEntity(ref _pathQueued);
                // });
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

        
    void FindNearestPointEntity(ref List<object[]> path)
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
