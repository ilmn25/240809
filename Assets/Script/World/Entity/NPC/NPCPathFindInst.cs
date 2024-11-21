using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;

public class NPCPathFindInst : MonoBehaviour
{ 
    public bool _repathRoutine = false;
    private NPCPathFindStatic _npcPathFindStatic;
    private PlayerMovementStatic _playerMovementStatic;
    private NPCMovementInst _npcMovementInst;
    private GameObject _player; 

    private bool _isPathFinding = false; 
    private bool _playerReached = false;
    private List<object[]> _path;
    private List<object[]> _pathQueued; 
    private int _nextPoint = 0;
    private int _nextPointQueued = -1;
    [HideInInspector] public float _pointDistance; 
    private float _playerDistance;
    private float _selfMoveDistance;
    private Vector3 _playerPositionPrevious;
    private Vector3 _selfPositionPrevious; 
    private bool _updatePlayerPosition = false;
    private bool _updateEntityPosition = false;
    private Vector3 _targetPoint;

    public int[] AGENT = new int[5];
    private float PLAYER_REACHED_INNER = 1f;
    private float PLAYER_REACHED_OUTER = 2f;
    // private float PLAYER_REACHED_PRECISION = 10;
    private float NEXT_POINT_DISTANCE = 0.45f;
    private float POINT_MISS_DISTANCE = 2f; 
    private float REPATH_INTERVAL = 0.1f; 
    private int JUMP_AHEAD = 1; 
    
    // private Vector3Int _target = new Vector3Int();
    // private bool _tracking = false;

    void Start()
    {
        _npcMovementInst = GetComponent<NPCMovementInst>();
        _npcPathFindStatic = GameObject.Find("world_system").GetComponent<NPCPathFindStatic>(); 
        _player = GameObject.Find("player");
        _playerMovementStatic =_player.GetComponent<PlayerMovementStatic>();
    }
     
    bool _moveOccupied = false;
    public async void HandlePathFindPassive()
    {  
        if (!_moveOccupied)
        {
            _moveOccupied = true;
            // try
            // { 
                
                _playerDistance = Vector3.Distance(transform.position, _player.transform.position);
                if (!_playerReached)
                {   
                    _playerReached = _playerDistance < PLAYER_REACHED_INNER;
                } else
                { 
                    _playerReached = _playerDistance < PLAYER_REACHED_OUTER;
                }

                if (!_playerReached)
                {
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
                } else _npcMovementInst._direction = Vector3.zero;

                _moveOccupied = false;
            // }
            // catch (Exception ex)
            // {
            //     if (ex is not MissingReferenceException && ex is not NullReferenceException )
            //     {
            //         throw new Exception("An exception occurred in HandlePathFindPassive method.", ex);
            //     }
            // }
        }
    }

    public void HandlePathFindActive()
    {
        if (!_repathRoutine) RepathRoutine();
 
        if (_updateEntityPosition && _npcMovementInst._isGrounded)
        { 
            _selfPositionPrevious = transform.position;
            _updateEntityPosition = false; 
        }
        if (_updatePlayerPosition && _playerMovementStatic._isGrounded) 
        {
            _playerPositionPrevious = _player.transform.position;
            _updatePlayerPosition = false;
        }
        
        _npcMovementInst._direction = Vector3.zero; 

        _playerDistance = Vector3.Distance(transform.position, _player.transform.position);
        if (!_playerReached)
        {   
            _playerReached = _playerDistance < PLAYER_REACHED_INNER;
        } else
        { 
            _path = null;
            _playerReached = _playerDistance < PLAYER_REACHED_OUTER;
        }
         
        if (_path != null 
        && !_playerReached 
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
                _targetPoint = (Vector3)_path[_nextPoint][0];  
                _pointDistance = Vector3.Distance(transform.position, _targetPoint); 
                if (_npcMovementInst._isGrounded && _pointDistance < NEXT_POINT_DISTANCE)
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
 
                    int potentialSkipPoint = _nextPoint + JUMP_AHEAD;
                    if (potentialSkipPoint < _path.Count - 1 
                    && !(bool)_path[potentialSkipPoint][1] 
                    && ((Vector3)_path[potentialSkipPoint][0]).y == ((Vector3)_path[_nextPoint][0]).y)
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
                        ) < NEXT_POINT_DISTANCE))
                    {
                        _nextPoint++;
                    }
                } 
                _npcMovementInst._direction = ((Vector3)_path[_nextPoint][0] - transform.position).normalized; 
            } else _npcMovementInst._direction = (Lib.AddToVector(_player.transform.position, 0, -0.3f, 0) - transform.position).normalized;
            
             
        } 
    }

 















 






    bool _playerMoved;
    private async void RepathRoutine()
    { 
        _repathRoutine = true;
        await Task.Delay((int)REPATH_INTERVAL * 1000); 
 
        if (!_playerReached && !_isPathFinding)
        {
            _playerMoved = Vector3.Distance(_playerPositionPrevious, _player.transform.position) > 0.8f;  //should be less than inner player near

            if (_playerMovementStatic._isGrounded && _playerMoved)
            { 
                GetPath();  
                _updatePlayerPosition = true;//! dont move
                // _playerPositionPrevious = _player.transform.position;
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
        if (_npcMovementInst._isGrounded) 
        { 
            if (_pointDistance > POINT_MISS_DISTANCE)
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
            _pathQueued = await _npcPathFindStatic.FindPath(AGENT, transform, _player.transform);  
            if (this != null)
            {
                _entityPosition = transform.position;
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

     
    // List<object[]> pathExtended;
    // List<object[]> toTargetPath;
    // List<object[]> toPlayerPath;
    // private async void ExtendPath()
    // {
    //     try
    //     { 
    //         if (_isPathFinding) return;

    //         _isPathFinding = true;
    
    //         Vector3Int target = (_target == Vector3Int.zero) ? Vector3Int.FloorToInt(_player.transform.position) : _target;

    //         pathExtended = _path; 

    //         _entityPosition = transform.position;
    //         int cutOffStart = Mathf.Min(_nextPoint, pathExtended.Count - 1);
    //         // int cutOffStart = await Task.Run(() => { return FindNearestPointEntity(ref _path);});
    //         cutOffStart = Mathf.Clamp(cutOffStart +1, 0, pathExtended.Count - 1);
    //         int cutOffEnd = await Task.Run(() => { return FindNearestPointTarget(cutOffStart, target, ref pathExtended);});

    //         toTargetPath = await _pathFindSystem.FindPath(AGENT, Vector3Int.FloorToInt((Vector3)pathExtended[cutOffEnd][0]), target);
    //         // toPlayerPath = await _pathFindSystem.FindPath(AGENT, Vector3Int.FloorToInt(transform.position), Vector3Int.FloorToInt((Vector3)pathExtended[cutOffStart][0]));

    //         pathExtended.RemoveRange(cutOffEnd, pathExtended.Count - cutOffEnd);
    //         if (toTargetPath != null) pathExtended.AddRange(toTargetPath);

    //         pathExtended.RemoveRange(0, Mathf.Max(cutOffStart - 1, 0));
    //         // if (toPlayerPath != null) pathExtended.InsertRange(0, toPlayerPath);
    
    //         _pathQueued = pathExtended; 
    //         _entityPosition = transform.position;
    //         _nextPointQueued = await Task.Run(() => { return FindNearestPointEntity(ref pathExtended);});


    //         _isPathFinding = false; 
    //     }
    //     catch (Exception ex)
    //     {
    //         if (ex is not MissingReferenceException && ex is not NullReferenceException )
    //         {
    //             throw new Exception("An exception occurred in SeparateMesh method.", ex);
    //         }
    //     }
    // }

    // int FindNearestPointTarget(int cutOffStart, Vector3Int target, ref List<object[]> path)
    // {
    //     try
    //     {   
    //         int bestPoint = cutOffStart;
    //         float bestDistance = Vector3.Distance((Vector3)target, (Vector3)path[cutOffStart][0]);

    //         for (int i = cutOffStart; i <= path.Count - 1; i++)
    //         {
    //             float currentDistance = Vector3.Distance((Vector3)target, (Vector3)path[i][0]) + i;

    //             if (currentDistance < bestDistance)
    //             {
    //                 bestPoint = i;
    //                 bestDistance = currentDistance;
    //             }
    //         }
    //         return bestPoint;
    //     }
    //     catch (Exception ex)
    //     {
    //         if (ex is not MissingReferenceException && ex is not NullReferenceException )
    //         {
    //             throw new Exception("An exception occurred in FindNearestPointTarget method.", ex);
    //         }
    //         return 0;
    //     }
    // }

    int _nearestPoint;
    Vector3 _entityPosition;
    float findNearestDistance;
    float findNearestNearestDistance;
    void FindNearestPointEntity(ref List<object[]> path)
    {
        // try
        // { 
            if (path != null && path.Count > 0) 
            {
                _nearestPoint = 0;
                findNearestNearestDistance = Vector3.Distance(_entityPosition, (Vector3)path[0][0]);
                for (int i = 1; i < path.Count; i++)
                {
                    findNearestDistance = Vector3.Distance(_entityPosition, (Vector3)path[i][0]);
                    if (findNearestDistance < findNearestNearestDistance)
                    {
                        _nearestPoint = i;
                        findNearestNearestDistance = findNearestDistance;
                    } else break;//TODO 
                } 
                if (_nearestPoint == 0) _nearestPoint = Mathf.Min(_nearestPoint + 1, path.Count -1); 
                _nextPointQueued =  _nearestPoint;
            }  
            _isPathFinding = false;
        // }
        // catch (Exception ex)
        // {
        //     CustomLibrary.Log("FindNearestPointEntity", ex);
        //     if (ex is not MissingReferenceException && ex is not NullReferenceException )
        //     {
        //         throw new Exception("An exception occurred in FindNearestPointEntity method.", ex);
        //     }
        //     _isPathFinding = false;
        // }
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
