using System.Collections.Generic;
using UnityEngine;

public class SmokeParticleHandler
{
    private static readonly Queue<GameObject> SmokeParticlePool = new Queue<GameObject>();
    private const int PoolSize = 10;
    private static int _smokeParticleCount = 0;
    private static GameObject _smokeParticle;

    static SmokeParticleHandler()
    {
        GameObject prefab = Resources.Load<GameObject>("Prefab/SmokePrefab");
        for (int i = 0; i < PoolSize; i++)
        {
            _smokeParticle = Object.Instantiate(prefab);
            _smokeParticle.SetActive(false);
            SmokeParticlePool.Enqueue(_smokeParticle);
        }
    }

    public static void CreateSmokeParticle(Vector3 smokePosition, bool force)
    {
        if (force || _smokeParticleCount < 5)
        {
            _smokeParticle = GetSmokeParticleFromPool();
            if (_smokeParticle)
            {
                _smokeParticle.SetActive(true);
                _smokeParticle.GetComponent<SmokeParticleComponent>().SpawnSmoke(smokePosition);
                if (!force) _smokeParticleCount++;
            }
        }
    }

    public static GameObject GetSmokeParticleFromPool()
    {
        if (SmokeParticlePool.Count > 0)
        {
            return SmokeParticlePool.Dequeue();
        }
        else return null;
    }

    public static void ReturnSmokeParticleToPool(GameObject smokeParticle)
    {
        smokeParticle.SetActive(false);
        SmokeParticlePool.Enqueue(smokeParticle);
        _smokeParticleCount--;
    }
}
