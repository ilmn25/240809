using System;
using System.Collections.Generic;

public abstract class State
{
    public String[] Tags;
    public StateMachine Root;
    public State Parent;

    protected List<State> states = new List<State>();
    protected State _stateCurrent;
    protected State _statePrevious;

    public virtual void OnEnterState() {}
    public virtual void OnUpdateState() {}
    public virtual void OnExitState() {}
 
    protected State(string[] tags = null)
    {
        Tags = tags;
    }
    
    public void OnUpdateInternal()
    {
        OnUpdateState();
        if (_stateCurrent != null)
        { 
            if (_stateCurrent != _statePrevious)
            {
                _stateCurrent.OnEnterState();
                _statePrevious.OnExitState();
                _statePrevious = _stateCurrent;
            }
            _stateCurrent.OnUpdateState();
        } 
    }
    
    protected void AddState(State state, Boolean current = false)
    {
        state.Root = Root;
        state.Parent = this;
        states.Add(state);
        if (current)
        {
            _stateCurrent = state;
            _statePrevious = state;
            _stateCurrent.OnEnterState();
        }
    }
    public void SetState<T>() where T : State
    {
        _stateCurrent = GetState<T>();
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
}