using System;
using System.Collections.Generic;
using UnityEngine;
 
public abstract class Machine : MonoBehaviour
{  
    private Dictionary<System.Type, Module> _modules = new Dictionary<System.Type, Module>();
    private List<State> _states = new List<State>();
    
    public virtual void OnInitialize() {}
    public virtual void OnUpdate() {}
    public virtual void OnTerminate() {}
 
    public void AddState(State state)
    {
        state.Machine = this; 
        _states.Add(state); 
    }
    
    public T GetState<T>() where T : State
    {
        foreach (var state in _states)
        {
            if (state is T)
            {
                return state as T;
            }
        }
        return null;
    }
    
    public T AddModule<T>(T module) where T : Module
    {
        module.Machine = this;
        _modules[module.GetType()] = module;
        return module;
    }
    
    public T GetModule<T>() where T : Module
    {
        foreach (var module in _modules.Values)
        {
            if (module is T)
            {
                return module as T;
            }
        }
        return null;
    }

    private void OnDestroy()
    {
        if (gameObject.activeSelf)
            TerminateMachine();
    }

    private void OnDisable()
    {
        TerminateMachine();
    }

    public void TerminateMachine()
    {
        OnTerminate();
        foreach (State state in _states)
        {
            state.OnTerminate();
        } 
        foreach (var module in _modules.Values)
        {
            module.Terminate();
        }
    }
 
    public void InitializeInteral()
    {
        _states.Clear();
        OnInitialize(); 
        foreach (var module in _modules.Values)
        {
            module.Initialize();
        }
        foreach (State state in _states)
        {
            state.OnInitialize(); 
            state.OnEnterInternal(); 
        }
    }
    
    protected virtual void Awake()
    {
        InitializeInteral();
    }
    
    private void Update()
    {
        OnUpdate(); 
        foreach (State state in _states)
        { 
            state.OnUpdateInternal();
        }
        foreach (Module module in _modules.Values)
        {
            module.Update();
        }
    }
 
}