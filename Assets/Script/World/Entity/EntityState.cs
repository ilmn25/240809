using System;
using UnityEngine;

public abstract class EntityState
{
    public String[] Tags;
    public EntityStateMachine StateMachine;
    protected EntityState(string[] tags = null)
    {
        Tags = tags;
    }

    public virtual void OnEnterState() {}
    public virtual void StateUpdate() {}
    public virtual void OnExitState() {}
}
 

class Idle : EntityState
{  
    public override void OnEnterState()
    { 
        WorldStatic.MapUpdated += OnMapUpdate;
    }
    public void OnMapUpdate(Vector3Int worldPosition)
    {
        if (Vector3Int.FloorToInt(StateMachine.transform.position) - new Vector3Int(0, 1, 0) == worldPosition)
        {
            StateMachine.WipeEntity();
        } 
    }
    public override void OnExitState()
    { 
        WorldStatic.MapUpdated -= OnMapUpdate;
    }
}