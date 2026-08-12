using System.Collections;
using UnityEngine;

public class CampfireMachine: CraftingMachine
{
    protected override bool GlowsWhenCrafting => true;

    private const float HealRadius = 6f;
    private const float HealInterval = 1f;
    private static readonly Collider[] HealBuffer = new Collider[32];

    private static readonly StatusEffect Cozy = new StatusEffect(
        ID.Campfire, EffectType.Heal, duration: 3f, tickInterval: 12f, amountPerTick: 1, name: "Cozy");

    public static Info CreateInfo()
    {
        return CraftInfo.CreateStructureInfo(ID.Campfire, 500, SfxID.HitStone, SfxID.HitStone);
    }

    public override void OnStart()
    {
        base.OnStart();
        StartCoroutine(HealRoutine());
    }

    private IEnumerator HealRoutine()
    {
        while (gameObject.activeSelf)
        {
            yield return new WaitForSeconds(HealInterval);
            if (!Helper.IsHost()) continue;
            if (Info.Destroyed) yield break;

            int count = Physics.OverlapSphereNonAlloc(transform.position, HealRadius, HealBuffer, Main.MaskEntity);
            for (int i = 0; i < count; i++)
            {
                if (!HealBuffer[i].TryGetComponent(out EntityMachine em)) continue;
                if (em.Info is not DynamicInfo dynamicInfo) continue;
                if (dynamicInfo.HitboxType == HitboxType.Enemy) continue;

                em.GetModule<StatusEffectModule>()?.Apply(Cozy);
            }
        }
    }
}
