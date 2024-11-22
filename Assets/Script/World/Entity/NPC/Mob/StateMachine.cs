using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class StateMachine : MonoBehaviour
{
    private List<State> states;
    private State state;
    private State statePrevious;

    protected void Initialize<T>(List<State> states) where T : State
    {
        this.states = states;
        this.state = GetState<T>();
        statePrevious = this.state;
    }

    public void Update()
    {
        LogicUpdate();
        if (state != statePrevious)
        {
            state.OnEnterState();
            statePrevious.OnExitState();
            statePrevious = state;
        }
        state.StateUpdate();
    }
    
    protected abstract void LogicUpdate();
    protected void SetState(State state)
    {
        this.state = state;
    }
    
    protected T GetState<T>() where T : State
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