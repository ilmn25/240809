using System.Collections.Generic;
using UnityEngine;

public enum Particles {Smoke, Dust}
public class Particle
{
    private static readonly Queue<GameObject> Pool = new ();
    private const int PoolSize = 20;
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

    public static void Create(Vector3 position, Particles id, bool force)
    {
        if (force || _count < PoolSize * 0.8f)
        {
            _particle = GetFromPool();
            if (_particle)
            {
                _particle.SetActive(true);
                _particle.GetComponent<ParticleComponent>().Spawn(position, id);
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
