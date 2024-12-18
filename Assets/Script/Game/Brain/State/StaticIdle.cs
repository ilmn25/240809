using System;
using UnityEngine;
 

class StaticIdle : State
{  
    public override void OnEnterState()
    { 
        World.MapUpdated += OnMapUpdate;
    }
    public void OnMapUpdate(Vector3Int worldPosition)
    {
        if (Vector3Int.FloorToInt(Machine.transform.position) + Vector3Int.down == worldPosition) // if block under is gone
        {
            ((EntityMachine) Machine).Delete();
        } 
    }
    public override void OnExitState()
    { 
        World.MapUpdated -= OnMapUpdate;
    }
}