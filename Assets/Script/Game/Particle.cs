using System.Collections.Generic;
using UnityEngine;

public enum Particles {Smoke, Dust,
    HitDust, Fire
}
public class Particle
{
    private static readonly Queue<GameObject> Pool = new ();
    private static readonly Dictionary<Particles, int> ParticleInfo = new()
    {
        [Particles.Smoke] = 2,
        [Particles.HitDust] = 2, 
        [Particles.Fire] = 0,
    };

    private const int PoolSize = 20;
    private const float DedupWindow = 0.15f;
    private static int _count = 0;
    private static GameObject _particle;

    // Dedup: (particleType, roundedPosition) → last creation time
    private static readonly Dictionary<(int, Vector3Int), float> _recentParticles = new();
 
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
        // Dedup: skip if the same particle type at the same position was created recently.
        // Catches duplicates from local simulation + host broadcast arriving on the same machine.
        Vector3Int roundedPos = Vector3Int.RoundToInt(position);
        var key = ((int)id, roundedPos);
        float now = Time.time;
        if (_recentParticles.TryGetValue(key, out float lastTime) && now - lastTime < DedupWindow)
            return;
        _recentParticles[key] = now;

        if (sync) EffectSync.EnqueueParticle(position, id, force);

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
                _particle.GetComponent<ParticleComponent>().Spawn(position, id, max);
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
