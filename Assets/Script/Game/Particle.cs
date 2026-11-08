using System.Collections.Generic;
using UnityEngine;

public enum Particles {Smoke, Dust,
    HitDust, Fire, BloodRed, BloodBlack, Lightning, None
}
public class Particle
{
    private static readonly Queue<GameObject> Pool = new ();
    private static readonly Dictionary<Particles, int> ParticleInfo = new()
    {
        [Particles.Smoke] = 2,
        [Particles.HitDust] = 2, 
        [Particles.Fire] = 0,
        [Particles.BloodRed] = 0,
        [Particles.BloodBlack] = 0,
        [Particles.Lightning] = 0,
    };

    private const int PoolSize = 40;
    private static int _count = 0;
    private static GameObject _particle;
 
    static Particle()
    {
        GameObject prefab = Resources.Load<GameObject>("Prefab/Particle");
        for (int i = 0; i < PoolSize; i++)
        {
            _particle = Object.Instantiate(prefab);
            _particle.SetActive(false);
            Pool.Enqueue(_particle);
        }
    }

    public static void Create(Vector3 position, Particles id, bool force, bool sync = true)
    {
        if (sync) EffectSync.EnqueueParticle(position, id, force);
        Spawn(position, null, id, force);
    }

    /// <summary>Spawn a particle that tracks the given entity for its whole lifetime,
    /// so fire on a moving mob stays on the mob instead of being left behind.</summary>
    public static void CreateAttached(EntityMachine anchor, Vector3 offset, Particles id, bool force)
    {
        if (id == Particles.None) return;
        if (anchor == null || anchor.Info == null) return;

        Vector3 position = anchor.transform.position + offset;
        EffectSync.EnqueueParticle(position, id, force, anchor.Info.uid);
        Spawn(position, anchor.transform, id, force);
    }

    /// <summary>Client-side spawn of an already-broadcast particle. When the follow
    /// uid resolves to a live entity the visual keeps tracking it; otherwise it
    /// stays where it spawned.</summary>
    public static void CreateSynced(Vector3 position, Particles id, bool force, string followUid)
    {
        Transform follow = null;
        if (!string.IsNullOrEmpty(followUid) &&
            EntitySync.InfoMap.TryGetValue(followUid, out Info info) &&
            info != null && info.Machine != null)
            follow = info.Machine.transform;

        Spawn(position, follow, id, force);
    }

    private static void Spawn(Vector3 position, Transform follow, Particles id, bool force)
    {
        if (id == Particles.None) return;

        int max = 0;
        ParticleInfo.TryGetValue(id, out max); 

        if (force || _count < PoolSize * 0.8f)
        {
            _particle = GetFromPool();
            if (_particle)
            {
                _particle.transform.position = new Vector3(0, 0, -0.05f);
                _particle.transform.rotation = Quaternion.identity;
                _particle.transform.localScale = Vector3.one;
                _particle.SetActive(true);
                _particle.GetComponent<ParticleComponent>().Spawn(position, follow, id, max);
                if (!force) _count++;
            }
        }
    }

    public static GameObject GetFromPool()
    {
        if (Pool.Count > 0)
        {
            return Pool.Dequeue();
        }
        return null;
    }

    public static void ReturnToPool(GameObject particle)
    {
        particle.SetActive(false);
        Pool.Enqueue(particle);
        _count--;
    }
}
