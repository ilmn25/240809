using UnityEngine;

/// <summary>
/// Makes an enemy bash down EnemyBreakable structures (doors, barricades) that
/// block its path, routing all damage through the normal hit system.
///
/// While a NavMap.Door cell sits between the enemy and its chase target, the enemy
/// is temporarily retargeted to that door (until the door opens or breaks). With the
/// door as the target the enemy's own AI chases it and, when adjacent, this module
/// forces the enemy's normal melee swing — which goes through the projectile hit
/// pipeline (OnHitInternal → AbstractHit → Damage) just like hitting any player.
///
/// The temporary target is restored as soon as the door is destroyed or the player
/// opens it (its NavMap cells turn to Air, so it no longer blocks).
/// </summary>
public class DoorBashModule : MobModule
{
    private static readonly Collider[] BashBuffer = new Collider[8];
    private const int BashScanCells = 3;   // how far ahead (in cells) to scan for a door
    private const int BashInterval = 25;   // frames between forced swings

    private int _swingCooldown;
    private Info _restoreTarget;
    private StructureInfo _door;

    public override void Update()
    {
        if (!Helper.IsHost()) return;

        if (_door != null)
        {
            if (_door.Destroyed || !IsDoorStillClosed(_door))
            {
                // Door is gone or open — go back to the original target.
                Info restore = _restoreTarget;
                _restoreTarget = null;
                _door = null;
                _swingCooldown = 0;
                if (!restore.Destroyed)
                {
                    Info.AcquireTarget(restore);
                    Info.PathingStatus = PathingStatus.Pending;
                }
                return;
            }

            // Damage only applies when the hit entity is the attacker's current target,
            // so keep it locked on the door even if the AI swaps targets after a hit.
            if (Info.Target != _door)
            {
                Info.AcquireTarget(_door);
                Info.PathingStatus = PathingStatus.Pending;
            }

            // Enemies without equipment (slimes, bugs) bash by contact instead.
            Info.AimPosition = _door.position;
            if (Info.Equipment != null && IsInAttackRange(_door))
            {
                if (_swingCooldown > 0) { _swingCooldown--; return; }
                _swingCooldown = BashInterval;
                ((MobMachine)Machine).Attack();
            }
            return;
        }

        if (Info.Target == null) return;

        StructureInfo door = FindBlockingDoor();
        if (door == null) return;

        _restoreTarget = Info.Target;
        _door = door;
        Info.AcquireTarget(door);
        Info.PathingStatus = PathingStatus.Pending;
    }

    private bool IsInAttackRange(StructureInfo door)
        => Vector3.Distance(Machine.transform.position, door.position) < Info.DistAttack + 0.5f;

    // A door counts as still-closed while any of its NavMap cells is still NavMap.Door
    // (opening the door writes Air to those cells, so bashing stops and the target is
    // restored).
    private bool IsDoorStillClosed(StructureInfo door)
    {
        Vector3Int baseCell = Vector3Int.FloorToInt(door.position);
        for (int y = 0; y < 2; y++)
            if (NavMap.Get(baseCell + new Vector3Int(0, y, 0)) == NavMap.Door)
                return true;
        return false;
    }

    private StructureInfo FindBlockingDoor()
    {
        Vector3 toTarget = Info.Target.position - Machine.transform.position;
        toTarget.y = 0f;
        float dist = toTarget.magnitude;
        if (dist < 1f) return null; // already adjacent — nothing left to bash
        Vector3 dir = toTarget / dist;

        int scan = Mathf.Min(BashScanCells, Mathf.CeilToInt(dist));
        Vector3Int start = Vector3Int.FloorToInt(Machine.transform.position) + Vector3Int.up; // torso height
        for (int step = 1; step <= scan; step++)
        {
            Vector3Int cell = start + Vector3Int.FloorToInt(dir * step);
            // Doors are 2 tall — check the bottom and top cell of the doorway.
            if (NavMap.Get(cell) == NavMap.Door || NavMap.Get(cell + Vector3Int.down) == NavMap.Door)
            {
                StructureInfo door = FindDoorEntityAt(cell);
                if (door != null) return door;
            }
        }
        return null;
    }

    private StructureInfo FindDoorEntityAt(Vector3Int cell)
    {
        Vector3 probe = cell + new Vector3(0.5f, 0.5f, 0.5f);
        int count = Physics.OverlapSphereNonAlloc(probe, 1f, BashBuffer, Main.MaskEntity);
        for (int i = 0; i < count; i++)
        {
            if (BashBuffer[i].TryGetComponent(out EntityMachine em) &&
                em.Info is StructureInfo si && si.EnemyBreakable && !si.Destroyed)
                return si;
        }
        return null;
    }
}
