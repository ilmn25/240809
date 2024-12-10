using System;
using System.Collections.Generic;
using UnityEngine;
 
public abstract class StateMachine : MonoBehaviour
{  
    protected List<State> states = new List<State>();
    protected State _stateCurrent;
    protected State _statePrevious;

    public virtual void OnAwake() {}
    public virtual void OnUpdate() {}

    private void Awake()
    {
        OnAwake();
    }
    public void Update()
    {
        OnUpdate();
        if (_stateCurrent != _statePrevious)
        {
            _stateCurrent.OnEnterState();
            _statePrevious.OnExitState();
            _statePrevious = _stateCurrent;
        }
        _stateCurrent.StateUpdate();
    } 
    protected void AddState(State state, Boolean current = false)
    {
        state.StateMachine = this;
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


public abstract class State
{
    public String[] Tags;
    public StateMachine StateMachine;
    public State SuperState;

    protected List<State> states = new List<State>();
    protected State _stateCurrent;
    protected State _statePrevious;

    public virtual void OnEnterState() {}
    public virtual void StateUpdate() {}
    public virtual void OnExitState() {}

    
    
    protected State(string[] tags = null)
    {
        Tags = tags;
    }
    
    public void OnUpdate()
    {
        StateUpdate();
        if (_stateCurrent != _statePrevious)
        {
            _stateCurrent.OnEnterState();
            _statePrevious.OnExitState();
            _statePrevious = _stateCurrent;
        }
        _stateCurrent.StateUpdate();
    }
    
    protected void AddState(State state, Boolean current = false)
    {
        state.StateMachine = StateMachine;
        state.SuperState = this;
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
