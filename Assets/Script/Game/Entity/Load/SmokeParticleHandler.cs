using System.Collections.Generic;
using UnityEngine;

public class SmokeParticleHandler
{
    private static Queue<GameObject> _smokeParticlePool = new Queue<GameObject>();
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
            _smokeParticlePool.Enqueue(_smokeParticle);
        }
    }

    public static void CreateSmokeParticle(Vector3 smokePosition, bool force)
    {
        if (force || _smokeParticleCount < 5)
        {
            _smokeParticle = GetSmokeParticleFromPool();
            if (_smokeParticle != null)
            {
                _smokeParticle.SetActive(true);
                _smokeParticle.GetComponent<SmokeParticleComponent>().SpawnSmoke(smokePosition);
                if (!force) _smokeParticleCount++;
            }
        }
    }

    public static GameObject GetSmokeParticleFromPool()
    {
        if (_smokeParticlePool.Count > 0)
        {
            return _smokeParticlePool.Dequeue();
        }
        else return null;
    }

    public static void ReturnSmokeParticleToPool(GameObject smokeParticle)
    {
        smokeParticle.SetActive(false);
        _smokeParticlePool.Enqueue(smokeParticle);
        _smokeParticleCount--;
    }
}
