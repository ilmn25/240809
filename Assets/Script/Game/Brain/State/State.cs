using System;
using System.Collections.Generic;
using UnityEngine;

public enum Tag
{
    Busy, Alert, Angry
}
public abstract class State
{
    public Tag[] Tags;
    public Machine Machine;
    public State Parent;
 
    public virtual void Initialize() {}
    public virtual void OnEnterState() {}
    public virtual void OnUpdateState() {}
    public virtual void OnExitState() {}
    
    public T Module<T>() where T : Module
    {
        return Machine.GetModule<T>();
    }
}

class DefaultState : State
{
}