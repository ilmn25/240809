using System;
using System.Collections.Generic;
using UnityEngine;
 
public abstract class Machine : MonoBehaviour
{  
    protected State State;
    private Dictionary<System.Type, Module> _modules = new Dictionary<System.Type, Module>();
    
    public virtual void OnInitialize() {}
    public virtual void OnUpdate() {}
    public virtual void OnTerminate() {}
 
    
    public void AddModule(Module module)
    {
        module.Machine = this;
        _modules[module.GetType()] = module;
        module.Initialize();
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

    public void TerminateMachine()
    {
        OnTerminate();
        State.OnTerminate();
        foreach (var module in _modules.Values)
        {
            module.Terminate();
        }
    }
 
    public void InitializeInteral()
    {
        OnInitialize();
        State.Machine = this; 
        State.OnEnterState();
    }
    
    protected virtual void Awake()
    {
        InitializeInteral();
    }
    
    private void Update()
    {
        OnUpdate(); 
        State.OnUpdateInternal(); 
    }
 
}