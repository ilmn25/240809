
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
 

public class GroundPathingModule : PathingModule
{  

    public override void OnStuck()
    {
        if (PathingTarget == PathingTarget.Target || PathingTarget == PathingTarget.Escape)
        {
            RepathCount++;
            if (RepathCount == MaxRepathCount)
            {
                Info.PathingStatus = PathingStatus.Stuck;
                Info.Direction = Vector3.zero;
                RepathCount = 0;
            }
            else 
                Repath();
        }
        else
        {
            Info.PathingStatus = PathingStatus.Stuck;
            Info.Direction = Vector3.zero;
        } 
    }

    protected override async Task<List<Node>> GetPath()
    {
        if (PathingTarget == PathingTarget.Target)
        {   
            // !Info.IsGrounded cant because grounded only lasts a frame || 
            while (Node.IsAir(Vector3Int.FloorToInt(Info.Target.position) + Vector3Int.down) ||
                   (!Info.IsGrounded && Node.IsAir(Vector3Int.FloorToInt(Info.position) + Vector3Int.down)))
            {
                await Task.Delay(100); 
            } 

            return await PathFind.FindPath(this, Info.PathAmount);
        }
        return PathRandom.FindPath(this);
    } 
    
    public override bool IsValidPosition(Vector3Int pos, Vector3Int dir, Node currentNode)
    { 
        //! check character size
        bool validity = true;
        for (int height = 0; height < Info.PathHeight; height++)
        { 
            if (!Node.IsAir(new Vector3Int(pos.x, pos.y + height, pos.z)))
            {
                validity = false;
            }
            if (!Node.IsAir(Vector3Int.FloorToInt(new Vector3(currentNode.Position.x, currentNode.Position.y + height + dir.y, currentNode.Position.z))))
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
            if (Node.IsAir(new Vector3Int(pos.x - dir.x, pos.y - dir.y, pos.z)) 
                && Node.IsAir(new Vector3Int(pos.x, pos.y - dir.y, pos.z - dir.z))
                && Node.IsAir(new Vector3Int(pos.x - dir.x, pos.y, pos.z))
                && Node.IsAir(new Vector3Int(pos.x, pos.y, pos.z - dir.z)))
            {
                validity = true;
            }
        } else {
            validity = true;
        }
        if (!validity) return false;
        
        //! check jump and air time
        if (Info.PathJump == 0) {
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
                for (int jump = 0; jump < Info.PathJump; jump++) // AGENT = 1 means jump 1 block, 2 means jump two block
                {
                    if (!Node.IsAir(new Vector3Int(pos.x, pos.y - jump, pos.z)))
                    {
                        validity = true;
                    }
                } 

                // check air time if not jumping currently
                if (!validity)
                {
                    if (Info.PathAir == -1)
                    {
                        validity = true;
                    }
                    else 
                    {
                        Node current = currentNode;
                        for (int i = 0; i < Info.PathAir && current != null; i++)
                        {
                            if ((dir.y == 0 || Info.PathJump > i) && !current.IsFloat)//TODO check if correct, air time if horizontal or double jump and not float more than jumps
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
            for (int fall = 0; fall < Info.PathFall; fall++) // AGENT = 1 means cant jump, 2 means jump one block
            {
                if (!Node.IsAir(new Vector3Int(pos.x, pos.y - fall, pos.z)))
                {
                    validity = true;
                }
            } 
        } else {
            validity = true;
        }
        if (!validity) return false;

        return true;
    }
}
