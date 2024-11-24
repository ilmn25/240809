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
}