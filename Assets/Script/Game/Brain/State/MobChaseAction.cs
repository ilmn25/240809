using UnityEngine;

class MobChaseAction : MobState {
    public override void OnEnterState()
    {
        if (Info.Target)
        { 
            Module<PathingModule>().SetTarget(PathingTarget.Target);
        } 
        else
            Machine.SetState<DefaultState>();
    }
    
    public override void OnUpdateState() {
        if (Game.PlayerInfo == Info && Info.ActionTarget != IActionTarget.Secondary)
        {
            Info.Target = null;
            Machine.SetState<DefaultState>(); 
            return;
        }
        if (Info.ActionTarget == IActionTarget.Follow && 
            (!Info.Target || Utility.SquaredDistance(Machine.transform.position, Info.Target.position) < Info.DistAttack * Info.DistAttack))
            Info.PathingStatus = PathingStatus.Reached;  
        
        if (Info.PathingStatus == PathingStatus.Reached)
        {
            if (Info.Target)
            { 
                if (Info.ActionTarget == IActionTarget.Secondary && Info.Target.gameObject.activeSelf)
                {
                    ((IActionSecondary)Info.Action).OnActionSecondary((EntityMachine) Machine);
                    Info.Target = null;
                    Machine.SetState<DefaultState>();
                } 
                else if (Info.ActionTarget == IActionTarget.Hit && Info.Target.gameObject.activeSelf)
                {
                    ((EntityMachine)Machine).Attack();
                }
                else
                {
                    Info.Target = null;
                    Machine.SetState<DefaultState>(); 
                } 
            }  
        }
        else if (Info.PathingStatus == PathingStatus.Stuck)
        {
            Info.Target = null;
            Machine.SetState<DefaultState>();
        }
    } 
}