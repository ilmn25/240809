using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Powers every structure within a 9-block radius. Powered structures
/// react via StructureMachine.OnPoweredChanged (lamps light, machines speed up).</summary>
public class GeneratorMachine : StructureMachine
{
    private const float PowerRange = 9f;
    private const float ScanInterval = 0.5f;
    private static readonly Collider[] PowerBuffer = new Collider[64];
    private readonly HashSet<StructureMachine> _poweredStructures = new();

    public static Info CreateInfo() => new StructureInfo
    {
        Health = 200,
        Loot = ID.Generator,
        SfxHit = SfxID.HitStone,
        SfxDestroy = SfxID.HitStone,
        operationType = OperationType.Cutting,
        GlowOn = true,
    };

    public override void OnStart()
    {
        base.OnStart();
        StartCoroutine(PowerRoutine());
    }

    private IEnumerator PowerRoutine()
    {
        while (gameObject.activeSelf)
        {
            yield return new WaitForSeconds(ScanInterval);
            if (!Helper.IsHost()) continue;
            if (Info.Destroyed)
            {
                UnpowerAll();
                yield break;
            }

            int count = Physics.OverlapSphereNonAlloc(transform.position, PowerRange, PowerBuffer, Main.MaskEntity);
            for (int i = 0; i < count; i++)
            {
                if (PowerBuffer[i].TryGetComponent(out StructureMachine sm) &&
                    sm != this && !sm.Info.Destroyed && _poweredStructures.Add(sm))
                    sm.SetPowered(true);
            }

            foreach (StructureMachine sm in new List<StructureMachine>(_poweredStructures))
            {
                if (sm.Info.Destroyed || Vector3.Distance(sm.transform.position, transform.position) > PowerRange)
                {
                    _poweredStructures.Remove(sm);
                    sm.SetPowered(false);
                }
            }
        }
    }

    private void UnpowerAll()
    {
        foreach (StructureMachine sm in _poweredStructures)
            sm.SetPowered(false);
        _poweredStructures.Clear();
    }
}
