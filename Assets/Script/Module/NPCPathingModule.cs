
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class NPCPathingModule : PathingModule
{ 
    public int HEIGHT = 1;
    public int JUMP = 1;
    public int FALL = 15;
    public int AIR = 3;
    public int ROAM = 15;
    public int SCAN = 7000;
    // 1 2 15 6 flea
    public override Vector3 GetTargetPosition()
    {
        if (Target)
            return Target.position;

        if (Path == null || Vector3.Distance(Machine.transform.position, Path[^1].Position) < 0.3f)
        {
            Lib.Log();
            Repath();
        } 
        
        return Path == null? Vector3.down : Path[^1].Position;
    }

    protected override async Task<List<Node>> GetPath()
    { 
        if (Target)
            return await PathFind.FindPath(this, SCAN); 
        return PathRandom.FindPath(this, ROAM); 
    } 
    
    public override bool IsValidPosition(Vector3Int pos, Vector3Int dir, Node currentNode)
    { 
        //! check character size
        bool validity = true;
        for (int height = 0; height < HEIGHT; height++)
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
        if (JUMP == 0) {
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
                for (int jump = 0; jump < JUMP; jump++) // AGENT = 1 means jump 1 block, 2 means jump two block
                {
                    if (!Node.IsAir(new Vector3Int(pos.x, pos.y - jump, pos.z)))
                    {
                        validity = true;
                    }
                } 

                // check air time if not jumping currently
                if (!validity)
                {
                    if (AIR == -1)
                    {
                        validity = true;
                    }
                    else 
                    {
                        Node current = currentNode;
                        for (int i = 0; i < AIR && current != null; i++)
                        {
                            if ((dir.y == 0 || JUMP > i) && !current.IsFloat)//TODO check if correct, air time if horizontal or double jump and not float more than jumps
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
            for (int fall = 0; fall < FALL; fall++) // AGENT = 1 means cant jump, 2 means jump one block
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
