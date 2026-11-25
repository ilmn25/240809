using System;
using UnityEngine;

/// <summary>Reusable entity scanning helpers that replace the repeated
/// OverlapSphereNonAlloc + "nearest match / count" loops scattered across
/// machines (threat scans, herd scans, grouping, aggro, etc.).</summary>
public static class EntityScan
{
    private const int BufferSize = 256;

    private static readonly Collider[] Buffer = new Collider[BufferSize];

    private static Collider[] Overlap(Vector3 origin, float radius, out int count)
    {
        count = Physics.OverlapSphereNonAlloc(origin, radius, Buffer, Main.MaskEntity);
        if (count < BufferSize) return Buffer;

        Collider[] overflow = Physics.OverlapSphere(origin, radius, Main.MaskEntity);
        count = overflow.Length;
        return overflow;
    }

    public static PlayerInfo FindNearestPlayer(Vector3 origin, float radius, Func<PlayerInfo, bool> predicate = null)
    {
        if (Save.Inst?.players == null) return null;

        PlayerInfo best = null;
        float bestSqr = radius * radius;
        foreach (PlayerInfo player in Save.Inst.players)
        {
            if (player == null || player.Destroyed || player.Health <= 0) continue;
            if (predicate != null && !predicate(player)) continue;

            float sqr = (player.position - origin).sqrMagnitude;
            if (sqr > bestSqr) continue;
            bestSqr = sqr;
            best = player;
        }
        return best;
    }

    /// <summary>Returns the nearest entity within radius whose Info matches the
    /// predicate, or null if none match.</summary>
    public static Info FindNearest(Vector3 origin, float radius, Func<Info, bool> predicate)
    {
        Collider[] hits = Overlap(origin, radius, out int count);
        Info best = null;
        float bestSqr = float.MaxValue;
        for (int i = 0; i < count; i++)
        {
            if (!hits[i].TryGetComponent(out EntityMachine em) || em.Info == null) continue;
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
        Collider[] hits = Overlap(origin, radius, out int count);
        int matches = 0;
        for (int i = 0; i < count; i++)
        {
            if (!hits[i].TryGetComponent(out EntityMachine em) || em.Info == null) continue;
            if (predicate(em.Info)) matches++;
        }
        return matches;
    }

    /// <summary>Runs the action for every entity within radius whose Info matches
    /// the predicate.</summary>
    public static void ForEach(Vector3 origin, float radius, Func<Info, bool> predicate, Action<Info> action)
    {
        Collider[] hits = Overlap(origin, radius, out int count);
        for (int i = 0; i < count; i++)
        {
            if (!hits[i].TryGetComponent(out EntityMachine em) || em.Info == null) continue;
            if (predicate(em.Info)) action(em.Info);
        }
    }
}
