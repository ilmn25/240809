using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine; 

public class NPCPathFindStatic : MonoBehaviour
{
    public static NPCPathFindStatic Instance { get; private set; }  

    public int MAX_SCAN_COUNT = 9000;
    public int MAX_TASK_COUNT = 1;
    private SemaphoreSlim _semaphore;

    void Start()
    {
        Instance = this;
        _semaphore = new SemaphoreSlim(MAX_TASK_COUNT, MAX_TASK_COUNT);
    } 
 
          
    Vector3Int _startPosition; 
    Vector3Int _endPosition; 
    public async Task<List<object[]>> FindPath(int[] _agent, Transform start, Transform end)
    {
        await _semaphore.WaitAsync();
        try
        { 
            if (start != null && end != null)
            {  
                // (_boolGridOrigin, _boolGrid, _startPosition, _endPosition) = _chunkSystem.PathFindMap(Vector3Int.FloorToInt(start.position), Vector3Int.FloorToInt(end.position));
                _startPosition = WorldStatic.Instance.GetRelativePosition(Vector3Int.FloorToInt(start.position));
                _endPosition = WorldStatic.Instance.GetRelativePosition(Vector3Int.FloorToInt(end.position)); 

                return await Task.Run(() => FindPathAsync(_agent, WorldStatic._boolMapOrigin, WorldStatic._boolMap, _startPosition, _endPosition));
            } 
            return null;
        }
        finally
        {
            _semaphore.Release();
        }
    } 
 
    private async Task<List<object[]>> FindPathAsync(int[] agent, Vector3Int gridOrigin, bool[,,] grid, Vector3Int startPosition, Vector3Int endPosition)
    {  
        int yieldCounter;
        bool validity;
        bool isClear;
        float gCost;
        float hCost;
        Vector3Int neighborPos;
        bool isFloat;
        Node neighborNode;
        float currentDistance;
        List<Node> openList; 
        HashSet<Vector3Int> closedList;
        Node closestNode;
        float closestDistance;
        Node currentNode;
        Vector3 adjustedPosition;
        List<object[]> pathList;
        object[] pathObject;

        closestDistance = 10000; 
        closestNode = null;
        openList = new List<Node>(); 
        closedList = new HashSet<Vector3Int>();
        yieldCounter = 0;
        try
        {
            // System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            // stopwatch.Start(); 
            openList.Add(new Node(startPosition, null, 0, Vector3Int.Distance(startPosition, endPosition), false, new Vector3Int(0, 0, 0)));
            // float _scanCount = Mathf.Max(MAX_SCAN_COUNT * Vector3Int.Distance(startPosition, endPosition) * 0.1f, 3000);
            while (openList.Count > 0 && closedList.Count < MAX_SCAN_COUNT)
            { 
                openList.Sort((A, B) => A.F.CompareTo(B.F)); // sort lowest f to [0] 
                currentNode = openList[0]; //set lowest f to current
                openList.RemoveAt(0); //remove current from open list
                // Debug.Log(currentNode.Direction + " at" + currentNode.Position);

                closedList.Add(currentNode.Position); //add to closed list

                if (currentNode.Position == endPosition) {
                    // stopwatch.Stop();
                    // Debug.Log($"Closest path found in {stopwatch.ElapsedMilliseconds} ms");
                    // UnityEngine.Debug.Log("Number of nodes searched: " + closedList.Count);
                    return RetracePath(currentNode); //found condition
                } 
                foreach (var direction in Directions) //scan each direction
                {
                    if (yieldCounter == 500)
                    {
                        yieldCounter = 0;
                        await Task.Yield(); // Yield control back to the main thread
                    } else yieldCounter++;

                    neighborPos = currentNode.Position + direction;
                    if (closedList.Contains(neighborPos) || !IsValidPosition(neighborPos, direction, currentNode)) {
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
                // await Task.Yield(); // Yield control back to the main thread
            }

            // if (agent[2] != 0) UnityEngine.Debug.Log("no complete path found");
            if (closestNode != null)
            { 
                // UnityEngine.Debug.Log("Number of nodes searched: " + closedList.Count);
                return RetracePath(closestNode); // Return the path to the closest node
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"An error occurred while finding the path: {ex.Message}");
        }
        return null; // No path found
 
        bool IsValidPosition(Vector3Int pos, Vector3Int dir, Node currentNode)
        {
            //! check character size
            validity = true;
            for (int height = 0; height < agent[1]; height++)
            { 
                if (!IsClear(new Vector3Int(pos.x, pos.y + height, pos.z)))
                {
                    validity = false;
                }
                if (!IsClear(new Vector3Int(currentNode.Position.x, currentNode.Position.y + height + dir.y, currentNode.Position.z)))
                {
                    validity = false;
                }
            } 
            if (!validity) return false;

            //! check for diagonal walls
            validity = false;
            if (dir.x != 0 && dir.z != 0)
            {  
                // deduct dir from pos because dir is the direction from current node to pos, not from pos to next pos            
                if (IsClear(new Vector3Int(pos.x - dir.x, pos.y - dir.y, pos.z)) 
                && IsClear(new Vector3Int(pos.x, pos.y - dir.y, pos.z - dir.z))
                && IsClear(new Vector3Int(pos.x - dir.x, pos.y, pos.z))
                && IsClear(new Vector3Int(pos.x, pos.y, pos.z - dir.z)))
                {
                    validity = true;
                }
            } else {
                validity = true;
            }
            if (!validity) return false;

            //! check jump and air time
            if (agent[2] == 0) {
                if (dir.y != 0) {
                    return false;
                }
            }
            else 
            {
                validity = false;
                if (dir.y >= 0) //only check if is jumping
                {
                    // check if going upward and have floor under to jump off
                    for (int jump = 0; jump < agent[2]; jump++) // agent = 1 means jump 1 block, 2 means jump two block
                    {
                        if (!IsClear(new Vector3Int(pos.x, pos.y - jump, pos.z)))
                        {
                            validity = true;
                        }
                    } 

                    // check air time if not jumping currently
                    if (!validity)
                    {
                        if (agent[4] == -1)
                        {
                            validity = true;
                        }
                        else 
                        {
                            Node current = currentNode;
                            for (int i = 0; i < agent[4] && current != null; i++)
                            {
                                if ((dir.y == 0 || agent[2] > i) && !current.IsFloat)//TODO check if correct, air time if horizontal or double jump and not float more than jumps
                                {
                                    validity = true;
                                    break;
                                }
                                current = current.Parent;
                            }
                        }
                    }
                    
                } else {
                    validity = true;
                }
                if (!validity) return false;
            }

            //! check fall distance, no suicide
            validity = false;
            if (dir.y == -1) //only check if is falling
            {
                for (int fall = 0; fall < agent[3]; fall++) // agent = 1 means cant jump, 2 means jump one block
                {
                    if (!IsClear(new Vector3Int(pos.x, pos.y - fall, pos.z)))
                    {
                        validity = true;
                    }
                } 
            } else {
                validity = true;
            }
            if (!validity) return false;

            // UnityEngine.Debug.Log(pos + " = " + validity);
            return true;
        }

        bool IsClear(Vector3Int pos)
        { 
            isClear = ( 
                pos.x >= 0 
                && pos.y >= 0 
                && pos.z >= 0 
                && pos.x < grid.GetLength(0) //x 
                && pos.z < grid.GetLength(2) //z 
            ); //check bounds x z 
            if (isClear)
            {
                isClear = pos.y >= grid.GetLength(1)? true : false; //check bounds y (true even if above max y)
                isClear = isClear ? true : grid[pos.x, pos.y, pos.z]; //check air or block
            } else isClear= false;
            return isClear;
        }
        
        List<object[]> RetracePath(Node endNode)
        {  
            Node currentNode = endNode;
            pathList = new List<object[]>();

            while (currentNode != null) 
            {
                // Adjust the node position and add 0.5 to each axis
                adjustedPosition = new Vector3(
                    currentNode.Position.x + gridOrigin.x + 0.5f,
                    currentNode.Position.y,
                    currentNode.Position.z + gridOrigin.z + 0.5f
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


    private class Node
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

    private Vector3Int[] Directions = 
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

}
