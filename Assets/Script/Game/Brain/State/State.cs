using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class State
{
    public String[] Tags;
    public Machine Machine;
    public State Parent;

    protected List<State> states = new List<State>();
    protected State _stateCurrent;
    protected State _statePrevious;

    public virtual void OnInitialize() {}
    public virtual void OnEnterState() {}
    public virtual void OnUpdateState() {}
    public virtual void OnExitState() {}
 
    protected State(string[] tags = null)
    {
        Tags = tags;
    }

    public void OnEnterInternal()
    {
        if (_stateCurrent != null)
            _stateCurrent.OnEnterInternal();
        OnEnterState();
    }
    public void OnExitInternal()
    {
        if (_stateCurrent != null)
            _stateCurrent.OnExitInternal();
        OnExitState();
    }
    
    public void OnUpdateInternal()
    {
        OnUpdateState(); 
        if (_stateCurrent != null)        
            _stateCurrent.OnUpdateInternal(); 
    }

    public void OnTerminate()
    {
        OnExitState();
        _stateCurrent?.OnTerminate();
    }
    
    protected void AddState(State state, Boolean current = false)
    {
        state.Machine = Machine;
        state.Parent = this;
        states.Add(state);
        state.OnInitialize();
        if (current)
        {
            _stateCurrent = state; 
            _stateCurrent.OnEnterInternal();
            _statePrevious = _stateCurrent;
        }
    }
    
    public void SetState<T>() where T : State
    {
        _stateCurrent = GetState<T>();
        
        if (_stateCurrent != _statePrevious)
        {
            _stateCurrent.OnEnterInternal();
            _statePrevious.OnExitInternal();
            _statePrevious = _stateCurrent;
        } 
    }
    
    public T GetState<T>() where T : State
    {
        foreach (var state in states)
        {
            if (state is T)
            {
                return state as T;
            }
        }
        return null;
    }

    public T GetModule<T>() where T : Module
    {
        return Machine.GetModule<T>();
    }

    public Type GetCurrentStateType()
    {
        return _stateCurrent?.GetType();
    }
}

class StateEmpty : State
{
}