
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
    public Info Info => ((EntityMachine)Machine).Info;
 
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
    public new MobInfo Info => (MobInfo)((EntityMachine)Machine).Info;
} 