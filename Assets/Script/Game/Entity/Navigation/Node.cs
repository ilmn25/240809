using System.Collections.Generic;
using UnityEngine;

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
    
    public static bool IsAir(Vector3Int pos)
    {
        if (pos.x < 0 || pos.y < 0 || pos.z < 0 || 
            pos.x >= World.Inst.Bounds.x || 
            pos.z >= World.Inst.Bounds.z)
            return false; // Check bounds x and z

        // Check bounds y (true even if above max y)
        if (pos.y >= World.Inst.Bounds.y)
            return true;

        // Check air or block
        return NavMap.Get(pos);
    }
    
 
    public List<Node> GetPathList()
    {  
        Node currentNode = this;
        List<Node> pathList = new List<Node>();

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
    
    public static readonly Vector3Int[] Directions = 
    {
        new (1, 0, 0),  //  right
        new (-1, 0, 0), //  left 
        new (0, 0, 1),  //  backward
        new (0, 0, -1), //  forward

        new (-1, 0, -1),//  forward-left
        new (1, 0, -1), //  forward-right
        new (-1, 0, 1), //  backward-left
        new (1, 0, 1),  //  backward-right 

        new (0, 1, 0),  //  up
        new (0, -1, 0), //  down
        
        new (-1, 1, 0), //  up-left 
        new (1, 1, 0),  //  up-right
        new (-1, -1, 0),//  down-left
        new (1, -1, 0), //  down-right

        new (0, 1, -1), //  up-forward
        new (0, 1, 1),  //  up-backward
        new (0, -1, -1),//  down-forward
        new (0, -1, 1), //  down-backward

        new (-1, 1, -1),//  up-left-forward
        new (-1, 1, 1),  //  up-left-backward
        new (1, 1, -1), //  up-right-forward
        new (1, 1, 1),  //  up-right-backward
        new (-1, -1, -1),// down-left-forward
        new (-1, -1, 1),//  down-left-backward
        new (1, -1, -1),//  down-right-forward
        new (1, -1, 1) //  down-right-backward
    };
    
    public static Vector3Int RandomDirection(int dist)
    {
        if (Random.value > 0.5f)
        {
            if (Random.value > 0.5f)
                return new Vector3Int(dist, -1, Random.Range(-dist, dist+1));
            
            return new Vector3Int(-dist, -1, Random.Range(-dist, dist+1));   
        }
        
        if (Random.value > 0.5f)
            return new Vector3Int(Random.Range(-dist, dist+1), -1, dist);
        
        return new Vector3Int(Random.Range(-dist, dist+1), -1, -dist);   
    }
}