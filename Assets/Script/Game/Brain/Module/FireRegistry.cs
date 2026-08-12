using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Host-side registry of all active flammable entities. Used by FlammableModule
/// to spread fire from a burning entity to nearby flammable neighbors.
/// </summary>
public static class FireRegistry
{
    private static readonly List<FlammableModule> _flammables = new List<FlammableModule>();

    public static void Register(FlammableModule module)
    {
        if (module == null) return;
        if (!_flammables.Contains(module))
            _flammables.Add(module);
    }

    public static void Unregister(FlammableModule module)
    {
        if (module == null) return;
        _flammables.Remove(module);
    }

    /// <summary>Remove any modules whose machine/info has been unloaded.</summary>
    public static void Prune()
    {
        for (int i = _flammables.Count - 1; i >= 0; i--)
        {
            FlammableModule module = _flammables[i];
            if (module == null || module.Machine == null || module.Info == null || module.Info.Destroyed)
                _flammables.RemoveAt(i);
        }
    }

    /// <summary>Attempt to ignite flammable objects near the given burning module.</summary>
    public static void SpreadFrom(FlammableModule source)
    {
        if (source == null || source.Info == null) return;
        Prune();

        Vector3 sourcePos = source.Info.position;
        float radius = source.SpreadRadius;
        float radiusSqr = radius * radius;

        for (int i = 0; i < _flammables.Count; i++)
        {
            FlammableModule target = _flammables[i];
            if (target == null || target == source) continue;
            if (target.Info == null || target.Info.Destroyed) continue;
            // Only spread to objects that aren't already burning.
            if (target.Info.FireLevel > 0f) continue;

            Vector3 delta = target.Info.position - sourcePos;
            if (delta.sqrMagnitude > radiusSqr) continue;

            if (Random.value <= source.SpreadChance)
                target.Ignite();
        }
    }

    /// <summary>Clear all registered flammables (world teardown).</summary>
    public static void Clear()
    {
        _flammables.Clear();
    }
}
