using System.Collections;
using UnityEngine;

class MobHit : MobState {

    public MobHit() { updateMode = global::Module.UpdateMode.Everyone; }
    private CoroutineTask _task;
    private const float HitTime = 1.267f;
    
    public override void OnEnterState()
    { 
        _task?.Stop();
        _task = null;
        Info.Animator.speed = 3f;  
        Info.Animator.Play("Hit", 0, 0f);   
        if (Main.PlayerInfo == Info)
            ScreenShake.Shake(40f, 0.05f, 1f / 60f);
        if (Helper.IsHost() || Info.IsOwner())
            _task = new CoroutineTask(HitRoutine());
    }
    
    private IEnumerator HitRoutine()
    {
        yield return new WaitForSeconds(HitTime / Info.Animator.speed);
        Info.Animator.speed = 1f;  
        Info.Animator.Play("EquipIdle", 0, 0f);
        Machine.SetState<DefaultState>();
    }

    public override void OnExitState()
    {
        _task?.Stop();
        _task = null;
        Info.Animator.speed = 1f;
        Info.Animator.Play("EquipIdle", 0, 0f); 
    }
}