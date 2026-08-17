using System.Collections;
using UnityEngine;

/// <summary>Periodically sprays water on nearby planters so crops grow without a bucket.</summary>
public class SprinklerMachine : StructureMachine
{
    private const float WaterRadius = 4f;
    private const float WaterInterval = 1f;
    private static readonly WaterSplashProjectileInfo Water = new() { Radius = WaterRadius };

    public static Info CreateInfo()
    {
        return new SpriteStructureInfo()
        {
            Health = 100,
            Loot = ID.Sprinkler,
            SfxHit = SfxID.HitMetal,
            SfxDestroy = SfxID.HitMetal,
            operationType = OperationType.Mining,
            threshold = 1,
        };
    }

    public override void OnStart()
    {
        base.OnStart();
        StartCoroutine(WaterRoutine());
    }

    private IEnumerator WaterRoutine()
    {
        while (gameObject.activeSelf)
        {
            yield return new WaitForSeconds(WaterInterval);
            if (!Helper.IsHost()) continue;
            if (Info.Destroyed) yield break;
            Projectile.Spawn(transform.position, transform.position + Vector3.up, Water, HitboxType.Passive, null);
        }
    }
}