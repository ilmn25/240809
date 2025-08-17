using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class PathRandom
{  
    public static List<Node> FindPath(PathingModule agent)
    {
        int scanCount; 
        Vector3Int offset;
        if (agent.PathingTarget == PathingTarget.Roam)
        {
            scanCount = agent.Info.DistRoam;
            offset = Node.RandomDirection(scanCount);
        } 
        else if(agent.PathingTarget == PathingTarget.Strafe)
        {
            scanCount = agent.Info.DistStrafe;
            offset = Node.RandomDirection(scanCount);
        } 
        else // if (agent.PathingTarget == PathingTarget.Escape)
        {
            scanCount = agent.PathingTarget == PathingTarget.Evade ? agent.Info.DistStrafe : agent.Info.DistEscape;
            
            Vector3 dest = agent.Machine.transform.position - agent.Info.Target.position;
            dest.y = 0;
            offset = Vector3Int.FloorToInt(dest.normalized * scanCount); 
        } 
        
        Vector3Int startPosition =
            NavMap.GetRelativePosition(Vector3Int.FloorToInt(agent.Machine.transform.position));
        Vector3Int endPosition =
            NavMap.GetRelativePosition(Vector3Int.FloorToInt(agent.Machine.transform.position + offset));

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
        
        openList.Add(new Node(startPosition, null, 0, Helper.SquaredDistance(startPosition, endPosition), false, Vector3Int.zero));
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
                if (!Scene.InPlayerChunkRange(World.GetChunkCoordinate(dirPosition), Scene.LogicDistance)) continue;  
                 
                isFloat = Node.IsAir(dirPosition + Vector3Int.down); // set true if midair
                gCost = currentNode.G + Helper.SquaredDistance(currentNode.Position, dirPosition);
                if (direction.x != 0 && direction.z != 0 && isFloat) gCost++;  
                  
                //don't add if not the lowest node compared to others
                if (openList.Exists(node => node.Position == dirPosition && node.G <= gCost) ||
                    !agent.IsValidPosition(dirPosition, direction, currentNode)) continue;
                
                hCost = Helper.SquaredDistance(dirPosition, endPosition); 
                neighborNode = new Node(dirPosition, currentNode, gCost, hCost, isFloat, direction);
                openList.Add(neighborNode);

                currentDistance = Helper.SquaredDistance(dirPosition, endPosition);
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