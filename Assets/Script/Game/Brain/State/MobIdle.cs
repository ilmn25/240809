using UnityEngine;

class MobIdle : MobState {
    private readonly int _idletime;
    private int _idletimer = 0;

    public MobIdle(int idletime = 300)
    {
        _idletime = idletime;
    }
 
    public override void OnEnterState()
    {
        Module<MobInfo>().Direction = Vector3.zero;
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