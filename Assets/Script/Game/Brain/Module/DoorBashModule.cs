using UnityEngine;

/// <summary>
/// Makes an enemy bash down EnemyBreakable structures (doors, barricades) that
/// block its path. While chasing a target, if a breakable door sits between the
/// enemy and its goal for a set amount of time, the door is acquired as the attack
/// target so the existing chase/attack system beats it down. Once the door breaks,
/// the enemy resumes chasing whatever it was after.
/// </summary>
public class DoorBashModule : MobModule
{
    private static readonly Collider[] BashBuffer = new Collider[8];
    private const float BashProbeDistance = 1.2f; // how far ahead of the mob to probe for a door
    private const float BashProbeRadius = 0.7f;
    private const int BashDelay = 90;             // frames stuck next to a door before bashing (~1.5s)

    private int _bashDelayTimer;
    private Info _restoreTarget;                  // the chase target we abandoned to bash the door

    public override void Update()
    {
        if (Info.HitboxType != HitboxType.Enemy) return;
        if (!Helper.IsHost()) return;

        // Bashing a door already: wait for it to break, then resume the chase.
        if (_restoreTarget != null)
        {
            if (Info.Target == null || Info.Target.Destroyed)
            {
                Info restored = _restoreTarget;
                _restoreTarget = null;
                _bashDelayTimer = 0;
                if (restored != null && !restored.Destroyed)
                {
                    Info.AcquireTarget(restored);
                    Info.PathingStatus = PathingStatus.Pending; // force re-path toward it
                }
            }
            return;
        }

        // Need a real chase target (not already attacking a structure).
        if (Info.Target == null || Info.Target is StructureInfo) { _bashDelayTimer = 0; return; }

        StructureInfo door = FindBlockingDoor();
        if (door == null) { _bashDelayTimer = 0; return; }

        // The door has been in the way this long — time to bash it down.
        _bashDelayTimer++;
        if (_bashDelayTimer < BashDelay) return;

        // Stuck next to a door for long enough — acquire it and bash it down.
        _bashDelayTimer = 0;
        _restoreTarget = Info.Target;
        Info.AcquireTarget(door);
        Info.PathingStatus = PathingStatus.Pending; // force re-path toward the door
    }

    // Looks for a breakable door directly between the enemy and its current target.
    private StructureInfo FindBlockingDoor()
    {
        Vector3 toTarget = Info.Target.position - Machine.transform.position;
        toTarget.y = 0f;
        if (toTarget.sqrMagnitude < 1.44f) return null; // already adjacent — melee handles it

        Vector3 probe = Machine.transform.position + Vector3.up * 0.5f + toTarget.normalized * BashProbeDistance;
        int count = Physics.OverlapSphereNonAlloc(probe, BashProbeRadius, BashBuffer, Main.MaskEntity);
        for (int i = 0; i < count; i++)
        {
            if (BashBuffer[i].TryGetComponent(out EntityMachine em) &&
                em.Info is StructureInfo si && si.EnemyBreakable && !si.Destroyed)
                return si;
        }
        return null;
    }
}
