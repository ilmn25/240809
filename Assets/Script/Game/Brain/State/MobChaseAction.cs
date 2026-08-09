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
        float engageRange = GetEngageRange();
        if (Info.ActionType != IActionType.PickUp && Info.IsGrounded &&
            (Info.Target == null || Helper.SquaredDistance(Machine.transform.position, Info.Target.position) < engageRange * engageRange))
            Info.PathingStatus = PathingStatus.Reached;

        // Target died/cleared or we're stuck — drop it and let the brain pick the next action.
        if (Info.Target == null || Info.PathingStatus == PathingStatus.Stuck)
        {
            Info.CancelTarget();
            return;
        }
        if (Info.PathingStatus != PathingStatus.Reached) return;

        switch (Info.ActionType)
        {
            case IActionType.Interact when Info.Target.Machine.gameObject.activeSelf:
                (Info.Target.Machine as IActionSecondaryInteract).OnActionSecondary(Info);
                Info.CancelTarget();
                break;
            case IActionType.PickUp when !Info.Target.Destroyed:
            {
                string targetUid = Info.Target.uid;
                (Info.Target as ItemInfo).OnActionSecondary(Info);
                Info.CancelTarget();
                // Client: queue destroy UID so the host removes the item server-side
                if (!Helper.IsHost() && NetworkClient.isConnected)
                    PlayerSync.SetPendingDestroyUid(targetUid);
                Machine.SetState<EquipSelectState>();
                break;
            }
            case IActionType.Hit or IActionType.Dig when !Info.Target.Destroyed:
                ((EntityMachine)Machine).Attack();
                break;
            default:
                Info.CancelTarget();
                break;
        }
    } 

    // How close the target must be before this action triggers:
    // followers trail closely, melee hits at arm's reach, ranged weapons fire from range.
    private float GetEngageRange()
    {
        if (Info.ActionType == IActionType.Follow) return Info.DistFollow;
        if (Info.Equipment != null && Info.Equipment.Info.Gesture == ItemGesture.Shoot) return Info.DistRanged;
        return Info.DistAttack;
    }
}