using UnityEngine;

class MobChaseAction : MobState {
    public override void OnEnterState()
    {
        if (Info.Target != null)
        { 
            Module<PathingModule>().SetTarget(PathingTarget.Target);
        } 
        else
            Machine.SetState<DefaultState>();
    }
    
    public override void OnUpdateState() {
        if (Main.PlayerInfo == Info && Info.ActionType != IActionType.Interact && Info.ActionType != IActionType.PickUp)
        {
            Info.CancelTarget();
            return;
        }
        if (Info.ActionType != IActionType.PickUp && Info.IsGrounded &&
            (Info.Target == null || Helper.SquaredDistance(Machine.transform.position, Info.Target.position) < Info.DistAttack * Info.DistAttack))
            Info.PathingStatus = PathingStatus.Reached;  
        
        if (Info.PathingStatus == PathingStatus.Reached)
        {
            if (Info.Target != null)
            { 
                if (Info.ActionType == IActionType.Interact && Info.Target.Machine.gameObject.activeSelf)
                {
                    (Info.Target.Machine as IActionSecondaryInteract).OnActionSecondary(Info);
                    Info.CancelTarget(); 
                } 
                else if (Info.ActionType == IActionType.PickUp && !Info.Target.Destroyed)
                {
                    (Info.Target as ItemInfo).OnActionSecondary(Info);
                    Info.CancelTarget();
                    Machine.SetState<EquipSelectState>();
                }
                else if (Info.ActionType == IActionType.Hit && !Info.Target.Destroyed)
                {
                    ((EntityMachine)Machine).Attack();
                }
                else if (Info.ActionType == IActionType.Dig && !Info.Target.Destroyed)
                {
                    ((EntityMachine)Machine).Attack();
                }
                else
                {
                    Info.CancelTarget();
                } 
            }  
        }
        else if (Info.PathingStatus == PathingStatus.Stuck)
        {
            Info.CancelTarget();
        }
    } 
}