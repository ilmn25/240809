using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine; 

public class PathFind 
{

    private static readonly int MaxTaskCount = 10;
    private static SemaphoreSlim _semaphore;
    static PathFind(){
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
                _startPosition = NavMap.GetRelativePosition(Vector3Int.FloorToInt(agent.Machine.transform.position));
                _endPosition = NavMap.GetRelativePosition(Vector3Int.FloorToInt(agent.GetTargetPosition())); 

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
        Vector3 delta;
        float currentDistance;
        float closestDistance = 1000; 
        Vector3Int dirPosition;
        
        float gCost;
        float hCost; 
        bool isFloat;  
         
        
        Node currentNode;
        Node closestNode = null;
        Node neighborNode; 
         
        openList.Add(new Node(startPosition, null, 0, Utility.SquaredDistance(startPosition, endPosition), false, new Vector3Int(0, 0, 0)));
        while (openList.Count > 0 && closedList.Count < scanCount)
        { 
            openList.Sort((A, B) => A.F.CompareTo(B.F)); // sort lowest f to [0] 
            currentNode = openList[0]; //set lowest f to current
            openList.RemoveAt(0); //remove current from open list
            closedList.Add(currentNode.Position); //add to closed list
            
            delta = endPosition - currentNode.Position;

            if ((Mathf.Abs(delta.x) == 1 && delta.y == 0 && delta.z == 0) ||
                (delta.x == 0 && Mathf.Abs(delta.y) == 1 && delta.z == 0) ||
                (delta.x == 0 && delta.y == 0 && Mathf.Abs(delta.z) == 1))
            {
                currentNode =  new Node(endPosition, currentNode, 0, 0, false,  Vector3Int.FloorToInt(endPosition - currentNode.Position));
                return currentNode.GetPathList();
            }  
            
            foreach (var direction in Node.Directions) //scan each direction
            {  
                dirPosition = Vector3Int.FloorToInt(currentNode.Position + direction);
                if (closedList.Contains(dirPosition)) continue;
                 
                isFloat = Node.IsAir(dirPosition + Vector3Int.down); // set true if midair
                gCost = currentNode.G + Utility.SquaredDistance(currentNode.Position, dirPosition);
                if (direction.x != 0 && direction.z != 0 && isFloat) gCost++;  
                  
                //don't add if not the lowest node compared to others
                if (openList.Exists(node => node.Position == dirPosition && node.G <= gCost) || 
                    !agent.IsValidPosition(dirPosition, direction, currentNode)) continue;   

                hCost = Utility.SquaredDistance(dirPosition, endPosition); 
                neighborNode = new Node(dirPosition, currentNode, gCost, hCost, isFloat, direction);
                openList.Add(neighborNode);

                currentDistance = Utility.SquaredDistance(dirPosition, endPosition);
                if (currentDistance < closestDistance)
                {
                    closestDistance = currentDistance;
                    closestNode = neighborNode;
                }
            }
        }

        return closestNode?.GetPathList(); // Return the path to the closest node 
    }
}