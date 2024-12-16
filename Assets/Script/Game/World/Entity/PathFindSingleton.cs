using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine; 

public class PathFindSingleton 
{

    private static readonly int MaxTaskCount = 10;
    private static SemaphoreSlim _semaphore;
    static PathFindSingleton(){
        _semaphore = new SemaphoreSlim(MaxTaskCount, MaxTaskCount);
    } 
           
          
    private static Vector3Int _startPosition; 
    private static Vector3Int _endPosition; 
    public static async Task<List<Node>> FindPath(PathingModule agent, int scanCount)
    {
        await _semaphore.WaitAsync();
        try
        { 
            if (agent.Machine.transform)
            {
                _startPosition = WorldSingleton.Instance.GetRelativePosition(Vector3Int.FloorToInt(agent.Machine.transform.position));
                _endPosition = WorldSingleton.Instance.GetRelativePosition(Vector3Int.FloorToInt(agent.GetTargetPosition())); 

                return await Task.Run(() => FindPathAsync(agent, _startPosition, _endPosition, scanCount));
            } 
            return null;
        }
        finally
        {
            _semaphore.Release();
        }
    } 
 
    private static List<Node> FindPathAsync(PathingModule agent, Vector3Int startPosition, Vector3Int endPosition, int scanCount)
    {  
        List<Node> openList = new List<Node>(); 
        HashSet<Vector3> closedList = new HashSet<Vector3>();
        
        float currentDistance;
        float closestDistance = 1000; 
        Vector3Int dirPosition;
        
        float gCost;
        float hCost; 
        bool isFloat;  
         
        
        Node currentNode;
        Node closestNode = null;
        Node neighborNode; 
          
        List<Node> pathList;
        
        try
        {
            openList.Add(new Node(startPosition, null, 0, Vector3Int.Distance(startPosition, endPosition), false, new Vector3Int(0, 0, 0)));
            while (openList.Count > 0 && closedList.Count < scanCount)
            { 
                openList.Sort((A, B) => A.F.CompareTo(B.F)); // sort lowest f to [0] 
                currentNode = openList[0]; //set lowest f to current
                openList.RemoveAt(0); //remove current from open list
                closedList.Add(currentNode.Position); //add to closed list
                
                //found condition
                if (currentNode.Position == endPosition) return RetracePath(currentNode);  
                
                foreach (var direction in _directions) //scan each direction
                {  
                    dirPosition = Vector3Int.FloorToInt(currentNode.Position + direction);
                    if (closedList.Contains(dirPosition) || !agent.IsValidPosition(dirPosition, direction, currentNode)) continue;
                     
                    isFloat = IsClear(dirPosition + Vector3Int.down); // set true if midair
                    gCost = currentNode.G + SquaredDistance(currentNode.Position, dirPosition);
                    if (direction.x != 0 && direction.z != 0 && isFloat) gCost++;  
                      
                    //dont add if not the lowest node compared to others
                    if (openList.Exists(node => node.Position == dirPosition && node.G <= gCost)) continue;   

                    hCost = SquaredDistance(dirPosition, endPosition); 
                    neighborNode = new Node(dirPosition, currentNode, gCost, hCost, isFloat, direction);
                    openList.Add(neighborNode);

                    currentDistance = SquaredDistance(dirPosition, endPosition);
                    if (currentDistance < closestDistance)
                    {
                        closestDistance = currentDistance;
                        closestNode = neighborNode;
                    }
                }
            }

            if (closestNode != null) return RetracePath(closestNode); // Return the path to the closest node
        }
        catch (Exception ex)
        {
            Debug.Log($"PATHFIND TASK ERROR: {ex.Message}");
        }
        return null; // No path found
  
 
        
        List<Node> RetracePath(Node endNode)
        {  
            Node currentNode = endNode;
            pathList = new List<Node>();

            while (currentNode != null)
            {
                currentNode.Position.x += 0.5f;
                currentNode.Position.z += 0.5f;

                pathList.Add(currentNode);
                currentNode = currentNode.Parent;
            }

            pathList.Reverse();  
            return pathList;
        }
    }
    
    public static float SquaredDistance(Vector3 a, Vector3 b)
    {
        return (a.x - b.x) * (a.x - b.x) + 
               (a.y - b.y) * (a.y - b.y) + 
               (a.z - b.z) * (a.z - b.z);
    }
    
    public static bool IsClear(Vector3Int pos)
    {
        if (pos.x < 0 || pos.y < 0 || pos.z < 0 || 
            pos.x >= WorldSingleton.World.Bounds.x || 
            pos.z >= WorldSingleton.World.Bounds.z)
            return false; // Check bounds x and z

        // Check bounds y (true even if above max y)
        if (pos.y >= WorldSingleton.World.Bounds.y)
            return true;

        // Check air or block
        return WorldSingleton._boolMap[pos.x, pos.y, pos.z];
    }

    private static Vector3Int[] _directions = 
    {
        new Vector3Int(1, 0, 0),  //  right
        new Vector3Int(-1, 0, 0), //  left 
        new Vector3Int(0, 0, 1),  //  backward
        new Vector3Int(0, 0, -1), //  forward

        new Vector3Int(-1, 0, -1),//  forward-left
        new Vector3Int(1, 0, -1), //  forward-right
        new Vector3Int(-1, 0, 1), //  backward-left
        new Vector3Int(1, 0, 1),  //  backward-right 

        new Vector3Int(0, 1, 0),  //  up
        new Vector3Int(0, -1, 0), //  down
        
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
 
}


public class Node
{
    public Vector3 Position;
    public Vector3Int Direction;
    public Node Parent;
    public float G;  
    public float H;  
    public float F => G + H;  
    public bool IsFloat;  

    public Node(Vector3 position, Node parent, float g, float h, bool isFloat, Vector3Int direction)
    {
        Position = position;
        Parent = parent;
        G = g;
        H = h;
        IsFloat = isFloat;
        Direction = direction;
    }
}