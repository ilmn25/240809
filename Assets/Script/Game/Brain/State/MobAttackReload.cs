using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

class MobAttackReload : MobState {

    public override void OnEnterState()
    {
        switch (Random.Range(1, 4))
        {
            case 1:
                Module<PathingModule>().SetTarget(PathingTarget.Strafe); 
                break;
            case 2:
                Module<PathingModule>().SetTarget(PathingTarget.Evade); 
                break;
            case 3:
                Module<PathingModule>().SetTarget(PathingTarget.Roam); 
                break;
        }
        
        Info.Animator.speed = 1.2f; 
        Info.Animator.Play("EquipReload", 0, 0f);  
        Audio.PlaySFX(Info.Equipment.Sfx);
    }
        
    public override void OnUpdateState() {
        if (Info.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
        {
            Audio.PlaySFX(Info.Equipment.Sfx);
            Info.Animator.speed = 1f;
            Info.Animator.Play("EquipIdle", 0, 0f);
            Machine.SetState<DefaultState>();
        } 
    }
}