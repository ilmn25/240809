using UnityEditor;
using UnityEngine;

class MobChase : MobState {
    public override void OnEnterState()
    {
        Module<PathingModule>().SetTarget(PathingTarget.Target);
    }
    
    public override void OnUpdateState() {
        if (Info.PathingStatus != PathingStatus.Pending)
        {
            Machine.SetState<DefaultState>();
            return;
        }
        if (!Info.Target || Vector3.Distance(Machine.transform.position, Info.Target.position) < Info.DistAttack)
            Info.PathingStatus = PathingStatus.Reached;  
    }
}

class MobChaseAim : MobState { 
    public override void OnEnterState()
    {
        Module<PathingModule>().SetTarget(PathingTarget.Target);
    }
    
    public override void OnUpdateState() {
        if (Info.PathingStatus != PathingStatus.Pending)
        {
            Machine.SetState<DefaultState>();
            return;
        }
        if (!Info.Target || InRangeAndVisible()) Info.PathingStatus = PathingStatus.Reached; 
    }
    

    bool InRangeAndVisible()
    {
        Vector3 origin = Machine.transform.position + Vector3.up * 0.3f; 
        float distance = Vector3.Distance(origin, Info.Target.transform.position);

        // Debug.DrawRay(origin, direction * distance, Color.red, 0.1f); // Lasts 0.1 seconds

        if (distance > Info.DistAttack) return false;

        if (Physics.Raycast(origin, (Info.Target.transform.position - origin).normalized,
                out RaycastHit hit, distance, Game.MaskMap))
        {
            return hit.distance >= distance - 0.2f;
        }
        
        return true;
    }
}
