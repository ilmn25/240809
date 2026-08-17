using System.Collections;
using UnityEngine;

/// <summary>Periodically waters nearby planters so crops grow without a bucket.</summary>
public class SprinklerMachine : StructureMachine
{
    private const float WaterRadius = 4f;
    private const float WaterInterval = 1f;
    private static readonly Collider[] Buffer = new Collider[32];

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

            int count = Physics.OverlapSphereNonAlloc(transform.position, WaterRadius, Buffer, Main.MaskEntity);
            for (int i = 0; i < count; i++)
            {
                if (!Buffer[i].TryGetComponent(out EntityMachine em)) continue;
                if (em is PlanterMachine planter)
                    planter.Water();
            }
        }
    }
}