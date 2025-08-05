
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class NPCPathingModule : PathingModule
{
    private readonly int _height;
    private readonly int _jump;
    private readonly int _fall;
    private readonly int _air;
    private readonly int _roam;
    private readonly int _scan;

    // 1 2 15 6 flea
    public NPCPathingModule(
        int height = 1,
        int jump = 1,
        int fall = 15,
        int air = 3,
        int roam = 15,
        int scan = 7000,
        float targetReachedInner = 0.5f, 
        float targetReachedOuter = 1.5f, 
        float pointReachDistance = 0.45f, 
        float repathInterval = 0.1f, 
        int jumpSkipAmount = 1)
        : base(
        targetReachedInner, 
        targetReachedOuter, 
        pointReachDistance, 
        repathInterval, 
        jumpSkipAmount)
    {
        _height = height;
        _jump = jump;
        _fall = fall;
        _air = air;
        _roam = roam;
        _scan = scan;
    }
 
    public override Vector3 GetTargetPosition()
    {
        if (Target)
            return Target.position;

        if (Path == null || Vector3.Distance(Machine.transform.position, Path[^1].Position) < 1)
            Repath();
        
        return Path == null? Vector3.down : Path[^1].Position;
    }

    protected override async Task<List<Node>> GetPath()
    {
        if (Target)
        {
            while (Node.IsAir(Vector3Int.FloorToInt(Target.position) + Vector3Int.down * _jump))
            {
                await Task.Delay(100); // Yield control back to the caller and continue checking
            }
            return await PathFind.FindPath(this, _scan); 
        } 
        return PathRandom.FindPath(this, _roam); 
    } 
    
    public override bool IsValidPosition(Vector3Int pos, Vector3Int dir, Node currentNode)
    { 
        //! check character size
        bool validity = true;
        for (int height = 0; height < _height; height++)
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
        if (_jump == 0) {
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
                for (int jump = 0; jump < _jump; jump++) // AGENT = 1 means jump 1 block, 2 means jump two block
                {
                    if (!Node.IsAir(new Vector3Int(pos.x, pos.y - jump, pos.z)))
                    {
                        validity = true;
                    }
                } 

                // check air time if not jumping currently
                if (!validity)
                {
                    if (_air == -1)
                    {
                        validity = true;
                    }
                    else 
                    {
                        Node current = currentNode;
                        for (int i = 0; i < _air && current != null; i++)
                        {
                            if ((dir.y == 0 || _jump > i) && !current.IsFloat)//TODO check if correct, air time if horizontal or double jump and not float more than jumps
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
            for (int fall = 0; fall < _fall; fall++) // AGENT = 1 means cant jump, 2 means jump one block
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
