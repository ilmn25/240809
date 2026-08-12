using System;
using UnityEngine;

/// <summary>Reusable entity scanning helpers that replace the repeated
/// OverlapSphereNonAlloc + "nearest match / count" loops scattered across
/// machines (threat scans, herd scans, grouping, aggro, etc.).</summary>
public static class EntityScan
{
    private static readonly Collider[] Buffer = new Collider[64];

    /// <summary>Returns the nearest entity within radius whose Info matches the
    /// predicate, or null if none match.</summary>
    public static Info FindNearest(Vector3 origin, float radius, Func<Info, bool> predicate)
    {
        int count = Physics.OverlapSphereNonAlloc(origin, radius, Buffer, Main.MaskEntity);
        Info best = null;
        float bestSqr = float.MaxValue;
        for (int i = 0; i < count; i++)
        {
            if (!Buffer[i].TryGetComponent(out EntityMachine em) || em.Info == null) continue;
            Info info = em.Info;
            if (!predicate(info)) continue;
            float sqr = (info.position - origin).sqrMagnitude;
            if (sqr < bestSqr) { bestSqr = sqr; best = info; }
        }
        return best;
    }

    /// <summary>Counts entities within radius whose Info matches the predicate.</summary>
    public static int Count(Vector3 origin, float radius, Func<Info, bool> predicate)
    {
        int count = Physics.OverlapSphereNonAlloc(origin, radius, Buffer, Main.MaskEntity);
        int matches = 0;
        for (int i = 0; i < count; i++)
        {
            if (!Buffer[i].TryGetComponent(out EntityMachine em) || em.Info == null) continue;
            if (predicate(em.Info)) matches++;
        }
        return matches;
    }
}
