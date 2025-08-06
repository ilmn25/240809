using UnityEngine;

class MobIdle : State {
    private MobStatusModule _mobStatusModule;
    private readonly int _idletime;
    private int _idletimer = 0;

    public MobIdle(int idletime = 100)
    {
        _idletime = idletime;
    }

    public override void OnInitialize()
    {
        _mobStatusModule = Machine.GetModule<MobStatusModule>();
    }

    public override void OnEnterState()
    {
        _mobStatusModule.Direction = Vector3.zero;
    }

    public override void OnUpdateState()
    {
        _idletimer++;
        if (_idletimer >= _idletime)
        { 
            _idletimer = 0;
            Parent.SetState<StateEmpty>();
        }
    }
} 