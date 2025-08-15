using System;

[System.Serializable]
public abstract class Module
{
    [NonSerialized] public Machine Machine;
    
    public virtual void Initialize() {}
    public virtual void Update() {}
    public virtual void Terminate() {}
    
    public T GetModule<T>() where T : Module
    {
        return Machine.GetModule<T>();
    }
}

public abstract class DynamicModule : Module
{
    protected DynamicInfo Info => (DynamicInfo)((EntityMachine) Machine).Info;
}

public abstract class MobModule : Module
{
    public MobInfo Info => (MobInfo)((EntityMachine)Machine).Info;
}

public abstract class PlayerModule : Module
{
    public PlayerInfo Info => (PlayerInfo)((EntityMachine) Machine).Info;
}

public abstract class MovementModule : DynamicModule { } 