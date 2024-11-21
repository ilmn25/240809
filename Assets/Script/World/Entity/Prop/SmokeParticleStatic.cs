using System.Collections.Generic;
using UnityEngine;

public class SmokeParticleStatic : MonoBehaviour
{
    public static SmokeParticleStatic Instance { get; private set; }  
    
    private static GameObject _smokeParticlePrefab;
    private static Queue<GameObject> _smokeParticlePool = new Queue<GameObject>();
    private static int _poolSize = 10;
    public static int _smokeParticleCount = 0;
    private static GameObject smokeParticle;

    void Awake()
    {
        Instance = this;
        _smokeParticlePrefab = Resources.Load<GameObject>("prefab/smoke_particle");

        // Initialize the pool
        for (int i = 0; i < _poolSize; i++)
        {
            smokeParticle = Instantiate(_smokeParticlePrefab);
            smokeParticle.transform.parent = transform;
            smokeParticle.SetActive(false);
            _smokeParticlePool.Enqueue(smokeParticle);
        }
    }

    public static void CreateSmokeParticle(Vector3 smokePosition, bool force)
    {
        if (force || _smokeParticleCount < 5)
        {
            smokeParticle = GetSmokeParticleFromPool();
            if (smokeParticle != null)
            {
                smokeParticle.SetActive(true);
                smokeParticle.GetComponent<SmokeParticleInst>().SpawnSmoke(smokePosition);
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
