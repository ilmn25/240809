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
[System.Serializable]
public abstract class EntityModule : Module
{
    public EntityMachine EntityMachine => (EntityMachine) Machine;
}
[System.Serializable]
public abstract class DynamicModule : EntityModule
{
    protected DynamicInfo Info => (DynamicInfo)EntityMachine.Info;
}
[System.Serializable]
public abstract class MobModule : EntityModule
{
    public MobInfo Info => (MobInfo)EntityMachine.Info;
}
[System.Serializable]
public abstract class PlayerModule : EntityModule
{
    public PlayerInfo Info => (PlayerInfo)EntityMachine.Info;
}
[System.Serializable]
public abstract class MovementModule : DynamicModule { } 