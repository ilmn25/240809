public abstract class Module
{
    public Machine Machine;
    
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
    protected DynamicInfo Info => (DynamicInfo) Machine.Info;
}

public abstract class MobModule : Module
{
    public MobInfo Info => (MobInfo) Machine.Info;
}

public abstract class PlayerModule : Module
{
    public PlayerInfo Info => (PlayerInfo) Machine.Info;
}

public abstract class MovementModule : DynamicModule { } 