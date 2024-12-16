using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class PathRandom
{  
    public static List<Node> FindPath(PathingModule agent, int scanCount)
    {
        Vector3Int startPosition =
            WorldSingleton.Instance.GetRelativePosition(Vector3Int.FloorToInt(agent.Machine.transform.position));

        Vector3Int endPosition =
            WorldSingleton.Instance.GetRelativePosition(Vector3Int.FloorToInt(agent.Machine.transform.position + 
                                                        Node.RandomDirection() * 100));

        List<Node> openList = new List<Node>(); 
        HashSet<Vector3> closedList = new HashSet<Vector3>();
        
        float currentDistance;
        float closestDistance = float.MaxValue; 
        Vector3Int dirPosition;
        
        float gCost;
        float hCost; 
        bool isFloat;  
        
        Node currentNode;
        Node closestNode = null;
        Node neighborNode;  
        
        openList.Add(new Node(startPosition, null, 0, Lib.SquaredDistance(startPosition, endPosition), false, Vector3Int.zero));
        while (openList.Count > 0 && closedList.Count < scanCount)
        { 
            openList.Sort((A, B) => A.F.CompareTo(B.F)); // sort lowest f to [0] 
            currentNode = openList[0]; //set lowest f to current
            openList.RemoveAt(0); //remove current from open list
            closedList.Add(currentNode.Position); //add to closed list
            
            foreach (var direction in Node.Directions) //scan each direction
            {  
                dirPosition = Vector3Int.FloorToInt(currentNode.Position + direction);
                if (closedList.Contains(dirPosition)) continue;
                 
                isFloat = Node.IsAir(dirPosition + Vector3Int.down); // set true if midair
                gCost = currentNode.G + Lib.SquaredDistance(currentNode.Position, dirPosition);
                if (direction.x != 0 && direction.z != 0 && isFloat) gCost++;  
                  
                //don't add if not the lowest node compared to others
                if (openList.Exists(node => node.Position == dirPosition && node.G <= gCost) ||
                    !agent.IsValidPosition(dirPosition, direction, currentNode)) continue;
                
                hCost = Lib.SquaredDistance(dirPosition, endPosition); 
                neighborNode = new Node(dirPosition, currentNode, gCost, hCost, isFloat, direction);
                openList.Add(neighborNode);

                currentDistance = Lib.SquaredDistance(dirPosition, endPosition);
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