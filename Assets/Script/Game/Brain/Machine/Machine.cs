using System;
using System.Collections.Generic;
using UnityEngine;
 
public abstract class Machine : MonoBehaviour
{  
    protected readonly List<Module> Modules = new List<Module>();
    protected readonly List<State> States = new List<State>();
    protected State StateCurrent = State.DefaultState;
    protected State StatePrevious = State.DefaultState; 
    
    public virtual void OnStart() {}
    public virtual void OnReset() {}
    public virtual void OnUpdate() {}
    public virtual void OnTerminate() {}
 
    public void AddState(State state, Boolean current = false)
    {
        state.Machine = this;
        States.Add(state);
        state.Initialize();
        if (current)
        {
            StateCurrent = state; 
            StateCurrent.OnEnterState();
            StatePrevious = StateCurrent;
        }
    }
    
    public void SetState<T>() where T : State
    {
        StateCurrent = GetState<T>();
        if (StateCurrent != StatePrevious)
        {
            StateCurrent.OnEnterState();
            StatePrevious.OnExitState();
            StatePrevious = StateCurrent;
        } 
    }
    
    public T GetState<T>() where T : State
    {
        foreach (var state in States)
        {
            if (state is T)
            {
                return state as T;
            }
        }
        if (StateCurrent == null) Debug.LogError("state not found: " + typeof(T).Name);
        return null;
    }
    
    public bool IsCurrentState<T>() where T : State
    {
        return StateCurrent is T;
    }
    
    public T AddModule<T>(T module) where T : Module
    {
        module.Machine = this;
        Modules.Add(module);
        module.Initialize();
        return module;
    }
    
    public T GetModule<T>() where T : Module
    {
        foreach (var module in Modules)
        {
            if (module is T)
            {
                return module as T;
            }
        }
        return null;
    }
 
    public void StartInternal()
    { 
        States.Add(State.DefaultState);
        OnStart(); 
    }
    
    public virtual void Update()
    {  
        OnUpdate();  
        if (StateCurrent != null) StateCurrent.OnUpdateState();
        foreach (Module module in Modules)
        {
            module.Update();
        }
    }
}

public class BasicMachine : Machine
{
    public void Start()
    { 
        StartInternal();
    }
}