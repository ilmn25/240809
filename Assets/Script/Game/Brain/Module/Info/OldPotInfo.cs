using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>A fragile old pot. Smashing it (with any tool) either pops out a
/// viper or spills loot — never both.</summary>
public class OldPotInfo : HarvestableInfo
{
    private const float ViperChance = 0.35f;

    protected override void OnHarvest(HarvestableDefinition definition)
    {
        if (Random.value < ViperChance)
        {
            Audio.PlaySFX(SfxID.HitStone);
            Entity.Spawn(ID.Viper, Vector3Int.FloorToInt(Machine.transform.position));
            Destroy();
        }
        else
        {
            base.OnHarvest(definition);
        }
    }
}
