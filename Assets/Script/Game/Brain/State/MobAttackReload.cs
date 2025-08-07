using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

class MobAttackReload : State {
    private PathingModule PathingModule => Machine.GetModule<PathingModule>();
    private MobStatusModule MobStatusModule => Machine.GetModule<MobStatusModule>();
    private Animator Animator => MobStatusModule.Animator;

    public override void OnEnterState()
    {
        switch (Random.Range(1, 4))
        {
            case 1:
                PathingModule.SetTarget(PathingTarget.Strafe); 
                break;
            case 2:
                PathingModule.SetTarget(PathingTarget.Evade); 
                break;
            case 3:
                PathingModule.SetTarget(PathingTarget.Roam); 
                break;
        }
        
        Animator.speed = 1.2f; 
        Animator.Play("EquipReload", 0, 0f);  
        Audio.PlaySFX(MobStatusModule.Equipment.Sfx, 0.5f);
    }
        
    public override void OnUpdateState() {
        if (Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
        {
            Audio.PlaySFX(MobStatusModule.Equipment.Sfx, 0.5f);
            Animator.speed = 1f;
            Animator.Play("EquipIdle", 0, 0f);
            Machine.SetState<DefaultState>();
        } 
    }
}