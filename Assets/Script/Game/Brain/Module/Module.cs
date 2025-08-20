using System;

[Serializable]
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

[Serializable]
public abstract class EntityModule : Module
{
    public EntityMachine EntityMachine => (EntityMachine)Machine;
}

[Serializable]
public abstract class DynamicModule : EntityModule
{
    private DynamicInfo _info;

    protected DynamicInfo Info
    {
        get
        {
            if (_info == null)
            {
                _info = (DynamicInfo)EntityMachine.Info;
            }
            return _info;
        }
    }
}

[Serializable]
public abstract class MobModule : EntityModule
{
    private MobInfo _info;

    public MobInfo Info
    {
        get
        {
            if (_info == null)
            {
                _info = (MobInfo)EntityMachine.Info;
            }
            return _info;
        }
    }
}

[Serializable]
public abstract class PlayerModule : EntityModule
{
    private PlayerInfo _info;

    public PlayerInfo Info
    {
        get
        {
            if (_info == null)
            {
                _info = (PlayerInfo)EntityMachine.Info;
            }
            return _info;
        }
    }
}

[Serializable]
public abstract class MovementModule : DynamicModule { }