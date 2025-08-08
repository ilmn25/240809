using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum Tag
{
    Busy, Alert, Angry
}
public abstract class State
{
    public static DefaultState DefaultState = new DefaultState();
    public Tag[] Tags;
    public Machine Machine;
    public State Parent;
    public Info Info => (Info) Machine.Info;
 
    public virtual void Initialize() {}
    public virtual void OnEnterState() {}
    public virtual void OnUpdateState() {}
    public virtual void OnExitState() {}
    
    public T Module<T>() where T : Module
    {
        return Machine.GetModule<T>();
    }
}

public class DefaultState : State { }

public class MobState : State
{
    public new MobInfo Info => (MobInfo) Machine.Info;
}
public class PlayerState : State
{
    public new PlayerInfo Info => (PlayerInfo) Machine.Info;
}