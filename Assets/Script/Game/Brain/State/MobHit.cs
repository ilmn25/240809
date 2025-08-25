using UnityEngine;

class MobHit : MobState {
    
    public override void OnEnterState()
    { 
        Info.Animator.speed = 3f;  
        Info.Animator.Play("Hit", 0, 0f);   
        ViewPort.StartScreenShake(1, 0.05f);
    }
    
    public override void OnUpdateState() {
        AnimatorStateInfo stateInfo = Info.Animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.normalizedTime >= 1f || !stateInfo.IsName("Hit"))
        {
            Info.Animator.speed = 1f;  
            Info.Animator.Play("EquipIdle", 0, 0f);
            Machine.SetState<DefaultState>(); 
        } 
    }

    public override void OnExitState()
    {
        Info.Animator.speed = 1f;
        Info.Animator.Play("EquipIdle", 0, 0f); 
    }
}