using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine; 

public class PathFindSingleton : MonoBehaviour
{
    public static PathFindSingleton Instance { get; private set; }  

    public int MAX_TASK_COUNT = 1;
    private SemaphoreSlim _semaphore;

    void Start()
    {
        Instance = this;
        _semaphore = new SemaphoreSlim(MAX_TASK_COUNT, MAX_TASK_COUNT);
    } 
          
    Vector3Int _startPosition; 
    Vector3Int _endPosition; 
    public async Task<List<object[]>> FindPath(PathFindModule agent, Transform start, Transform end, int scanCount)
    {
        await _semaphore.WaitAsync();
        try
        { 
            if (start)
            {
                _startPosition = WorldStatic.Instance.GetRelativePosition(Vector3Int.FloorToInt(start.position));
                if (!end)
                    _endPosition = _startPosition + new Vector3Int(
                       UnityEngine.Random.Range(-scanCount, scanCount),  
                       UnityEngine.Random.Range(-1, 1),  
                       UnityEngine.Random.Range(-scanCount, scanCount));
                else 
                    _endPosition = WorldStatic.Instance.GetRelativePosition(Vector3Int.FloorToInt(end.position)); 

                return await Task.Run(() => FindPathAsync(agent, _startPosition, _endPosition, scanCount));
            } 
            return null;
        }
        finally
        {
            _semaphore.Release();
        }
    } 
 
    private async Task<List<object[]>> FindPathAsync(PathFindModule agent, Vector3Int startPosition, Vector3Int endPosition, int scanCount)
    {  
        float gCost;
        float hCost;
        Vector3Int neighborPos;
        bool isFloat;
        Node neighborNode;
        float currentDistance;
        
        List<Node> openList = new List<Node>(); 
        HashSet<Vector3Int> closedList = new HashSet<Vector3Int>();
        
        Node closestNode = null;
        float closestDistance = 10000; 
        
        Node currentNode;
        Vector3 adjustedPosition;
        List<object[]> pathList;
        object[] pathObject;
        
        try
        {
            openList.Add(new Node(startPosition, null, 0, Vector3Int.Distance(startPosition, endPosition), false, new Vector3Int(0, 0, 0)));
            while (openList.Count > 0 && closedList.Count < scanCount)
            { 
                openList.Sort((A, B) => A.F.CompareTo(B.F)); // sort lowest f to [0] 
                currentNode = openList[0]; //set lowest f to current
                openList.RemoveAt(0); //remove current from open list

                closedList.Add(currentNode.Position); //add to closed list

                if (currentNode.Position == endPosition) {
                    return RetracePath(currentNode); //found condition
                } 
                foreach (var direction in _directions) //scan each direction
                {
                    if (closedList.Count == 500)
                    {
                        await Task.Yield(); // Yield control back to the main thread
                    }  

                    neighborPos = currentNode.Position + direction;
                    if (closedList.Contains(neighborPos) || !agent.IsValidPosition(neighborPos, direction, currentNode)) {
                        continue;
                    }
                     
                    isFloat = IsClear(new Vector3Int(neighborPos.x, neighborPos.y -1, neighborPos.z)); // set true if mid air
                    if (direction.x != 0 && direction.z != 0 && isFloat)
                    {
                        gCost = currentNode.G + Vector3Int.Distance(currentNode.Position, neighborPos) + 1;
                    } else
                    {
                        gCost = currentNode.G + Vector3Int.Distance(currentNode.Position, neighborPos);
                    }
                    hCost = Vector3Int.Distance(neighborPos, endPosition);
                    neighborNode = new Node(neighborPos, currentNode, gCost, hCost, isFloat, direction);

                    if (openList.Exists(node => node.Position == neighborPos && node.G <= gCost)) {
                        continue; //dont add if not the lowest node compared to others 
                    }

                    openList.Add(neighborNode);

                    currentDistance = Vector3Int.Distance(neighborPos, endPosition);
                    if (currentDistance < closestDistance)
                    {
                        closestDistance = currentDistance;
                        closestNode = neighborNode;
                    }
                }
            }

            if (closestNode != null)
            { 
                return RetracePath(closestNode); // Return the path to the closest node
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"An error occurred while finding the path: {ex.Message}");
        }
        return null; // No path found
  
 
        
        List<object[]> RetracePath(Node endNode)
        {  
            Node currentNode = endNode;
            pathList = new List<object[]>();

            while (currentNode != null) 
            {
                // Adjust the node position and add 0.5 to each axis
                adjustedPosition = new Vector3(
                    currentNode.Position.x + 0.5f,
                    currentNode.Position.y,
                    currentNode.Position.z + 0.5f
                );

                // Create an array with adjustedPosition and currentNode.isFloat
                pathObject = new object[] { adjustedPosition, currentNode.IsFloat};

                pathList.Add(pathObject);
                currentNode = currentNode.Parent;
            }

            pathList.Reverse();  
            return pathList;
        }
    }
 

    private Vector3Int[] _directions = 
    {
        new Vector3Int(1, 0, 0),  //  right
        new Vector3Int(-1, 0, 0), //  left
        new Vector3Int(0, 1, 0),  //  up
        new Vector3Int(0, -1, 0), //  down
        new Vector3Int(0, 0, 1),  //  backward
        new Vector3Int(0, 0, -1), //  forward

        new Vector3Int(-1, 0, -1),//  forward-left
        new Vector3Int(1, 0, -1), //  forward-right
        new Vector3Int(-1, 0, 1), //  backward-left
        new Vector3Int(1, 0, 1),  //  backward-right 

        new Vector3Int(-1, 1, 0), //  up-left 
        new Vector3Int(1, 1, 0),  //  up-right
        new Vector3Int(-1, -1, 0),//  down-left
        new Vector3Int(1, -1, 0), //  down-right

        new Vector3Int(0, 1, -1), //  up-forward
        new Vector3Int(0, 1, 1),  //  up-backward
        new Vector3Int(0, -1, -1),//  down-forward
        new Vector3Int(0, -1, 1), //  down-backward

        new Vector3Int(-1, 1, -1),//  up-left-forward
        new Vector3Int(-1, 1, 1),  //  up-left-backward
        new Vector3Int(1, 1, -1), //  up-right-forward
        new Vector3Int(1, 1, 1),  //  up-right-backward
        new Vector3Int(-1, -1, -1),// down-left-forward
        new Vector3Int(-1, -1, 1),//  down-left-backward
        new Vector3Int(1, -1, -1),//  down-right-forward
        new Vector3Int(1, -1, 1) //  down-right-backward
    };

    public static bool IsClear(Vector3Int pos)
    { 
        bool isClear = ( 
            pos.x >= 0 
            && pos.y >= 0 
            && pos.z >= 0 
            && pos.x < WorldStatic.World.Bounds.x 
            && pos.z < WorldStatic.World.Bounds.z 
        ); //check bounds x z 
        if (isClear)
        {
            isClear = pos.y >= WorldStatic._boolMap.GetLength(1)? true : false; //check bounds y (true even if above max y)
            isClear = isClear || WorldStatic._boolMap[pos.x, pos.y, pos.z]; //check air or block
        }
        return isClear;
    }
}


public class Node
{
    public Vector3Int Position;
    public Vector3Int Direction;
    public Node Parent;
    public float G;  
    public float H;  
    public float F => G + H;  
    public bool IsFloat;  

    public Node(Vector3Int position, Node parent, float g, float h, bool isFloat, Vector3Int direction)
    {
        Position = position;
        Parent = parent;
        G = g;
        H = h;
        IsFloat = isFloat;
        Direction = direction;
    }
}