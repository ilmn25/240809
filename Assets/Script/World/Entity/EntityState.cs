using System;

public abstract class EntityState
{
    public String[] Tags;

    protected EntityState(string[] tags = null)
    {
        Tags = tags;
    }

    public abstract void OnEnterState();
    public abstract void StateUpdate();
    public abstract void OnExitState();
}
 