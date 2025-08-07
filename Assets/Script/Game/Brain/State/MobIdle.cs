using UnityEngine;

class MobIdle : State {
    private readonly int _idletime;
    private int _idletimer = 0;

    public MobIdle(int idletime = 400)
    {
        _idletime = idletime;
    }
 
    public override void OnEnterState()
    {
        Module<MobStatusModule>().Direction = Vector3.zero;
    }

    public override void OnUpdateState()
    {
        _idletimer++;
        if (_idletimer >= _idletime)
        { 
            _idletimer = 0;
            Machine.SetState<DefaultState>();
        }
    }
} 