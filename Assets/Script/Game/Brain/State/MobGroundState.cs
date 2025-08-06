using Newtonsoft.Json.Linq;
using UnityEngine;

public class MobGroundState : State
{ 
    MobStatusModule _mobStatusModule;
    public override void OnEnterState()
    {
        _mobStatusModule = Machine.GetModule<MobStatusModule>();
        AddState(new StateEmpty(), true);
        AddState(new MobIdle());
        AddState(new MobChase());
        AddState(new MobRoam());
    }
    
    public override void OnUpdateState()
    {
        HandleInput();

        if (IsCurrentState<StateEmpty>())
        {
            if (_mobStatusModule.Target)
            {
                if (Vector3.Distance(_mobStatusModule.Target.transform.position, Machine.transform.position) < 3)
                    SetState<MobAttack>();
                else if (_mobStatusModule.PathingStatus == PathingStatus.Stuck)
                {
                    SetState<MobRoam>();
                }
                else
                {
                    SetState<MobChase>();
                }
            }
            else
            { 
                if (Random.value > 0.5f)
                    SetState<MobRoam>();
                else
                    SetState<MobIdle>();
            }
        }
    }
    
    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Y))
            _mobStatusModule.Target = Game.Player.transform;
        else if (Input.GetKeyDown(KeyCode.T))
            _mobStatusModule.Target = null;
        else if (Input.GetKeyDown(KeyCode.U))
            Machine.transform.position = Game.Player.transform.position;
    }
}