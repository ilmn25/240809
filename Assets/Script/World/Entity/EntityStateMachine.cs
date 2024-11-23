using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class EntityStateMachine : MonoBehaviour
{
    private List<EntityState> states = new List<EntityState>();
    private EntityState _entityState;
    private EntityState _entityStatePrevious;

    protected void AddState(EntityState state, Boolean current = false)
    {
        states.Add(state);
        if (current)
        {
            _entityState = state;
            _entityStatePrevious = state;
        }
    }

    public void Update()
    {
        LogicUpdate();
        if (_entityState != _entityStatePrevious)
        {
            _entityState.OnEnterState();
            _entityStatePrevious.OnExitState();
            _entityStatePrevious = _entityState;
        }
        _entityState.StateUpdate();
    }

    public abstract void OnEnable();
    protected abstract void LogicUpdate();
    public void SetState(EntityState entityState)
    {
        _entityState = entityState;
    }

    public void SetState<T>() where T : EntityState
    {
        _entityState = GetState<T>();
    }
    
    public T GetState<T>() where T : EntityState
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