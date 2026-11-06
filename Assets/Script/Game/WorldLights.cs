using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Registry of the world's local light sources — lamps, forges, fires — so the
/// player's permanent spotlight can brighten while standing in their glow. A source
/// registers once when its machine is set up; its state is read live, so switching a
/// lamp on or off needs no extra bookkeeping.
/// </summary>
public static class WorldLights
{
    private static readonly List<Light> Sources = new List<Light>();

    public static void Register(Light light)
    {
        if (light == null || Sources.Contains(light)) return;
        Sources.Add(light);
    }

    /// <summary>Brightness of the nearest lit source at <paramref name="position"/>:
    /// 1 at the source, easing to 0 at the edge of its range. A source's own range is
    /// what limits it, so authors tune reach in the prefab.</summary>
    public static float GlowAt(Vector3 position)
    {
        float glow = 0f;
        for (int i = Sources.Count - 1; i >= 0; i--)
        {
            Light source = Sources[i];
            // Pooled objects can be destroyed without unregistering; drop dead entries.
            if (source == null)
            {
                Sources.RemoveAt(i);
                continue;
            }
            if (!source.isActiveAndEnabled || source.intensity <= 0f)
                continue;

            float range = source.range;
            if (range <= 0f) continue;

            float sqrDistance = (source.transform.position - position).sqrMagnitude;
            if (sqrDistance >= range * range) continue;

            float closeness = 1f - Mathf.Sqrt(sqrDistance) / range;
            glow = Mathf.Max(glow, Mathf.SmoothStep(0f, 1f, closeness));
        }
        return glow;
    }
}
