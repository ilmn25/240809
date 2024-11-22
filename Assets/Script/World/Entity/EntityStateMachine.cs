using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class EntityStateMachine : MonoBehaviour
{
    private List<EntityState> states;
    private EntityState _entityState;
    private EntityState _entityStatePrevious;

    protected void Initialize<T>(List<EntityState> states) where T : EntityState
    {
        this.states = states;
        this._entityState = GetState<T>();
        _entityStatePrevious = this._entityState;
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
    
    protected abstract void LogicUpdate();
    public void SetState(EntityState entityState)
    {
        this._entityState = entityState;
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