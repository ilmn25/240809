using System;
using UnityEngine;
 

class Idle : State
{  
    public override void OnEnterState()
    { 
        WorldSingleton.MapUpdated += OnMapUpdate;
    }
    public void OnMapUpdate(Vector3Int worldPosition)
    {
        if (Vector3Int.FloorToInt(Root.transform.position) - new Vector3Int(0, 1, 0) == worldPosition)
        {
            ((EntityMachine) Root).WipeEntity();
        } 
    }
    public override void OnExitState()
    { 
        WorldSingleton.MapUpdated -= OnMapUpdate;
    }
}