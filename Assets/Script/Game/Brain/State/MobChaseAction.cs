using Mirror;
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
        // Teleport followers to target if out of logic range
        if (Info.ActionType == IActionType.Follow && Info.Target?.Machine != null &&
            Vector3.Distance(Machine.transform.position, Info.Target.position) > Scene.LogicDistance)
        {
            Machine.transform.position = Info.Target.position;
            return;
        }

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
                    string targetUid = Info.Target.uid;
                    (Info.Target as ItemInfo).OnActionSecondary(Info);
                    Info.CancelTarget();
                    // Client: queue destroy UID so the host removes the item server-side
                    if (!Helper.IsHost() && NetworkClient.isConnected)
                        PlayerSync.SetPendingDestroyUid(targetUid);
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