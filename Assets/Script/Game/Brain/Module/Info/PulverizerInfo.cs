using System;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>Crafting info for a pulverizer: queuing a geode consumes it and, once
/// the crush finishes, yields a random ore or fossil instead of a fixed product.</summary>
[Serializable]
public class PulverizerInfo : ConverterInfo
{
    private static readonly ID[] RandomResults =
    {
        ID.Copper,
        ID.Steel,
        ID.Slag,
        ID.Charcoal,
        ID.Fossil,
    };

    [NonSerialized] private int _crushCounter;

    public override void Update()
    {
        if (Pending.Count == 0)
            return;

        if (Sfx != SfxID.Null)
            Audio.PlaySFX(Sfx);

        int time = ItemRecipe.GetRecipe(ID.Geode)?.Time ?? 90;
        if (_crushCounter >= time)
        {
            Vector3 offset = new Vector3(
                Random.value > 0.5f ? 0.65f : -0.65f,
                1.8f,
                Random.value > 0.5f ? 0.65f : -0.65f);

            Entity.SpawnItem(RandomResults[Random.Range(0, RandomResults.Length)], Machine.transform.position + offset, stackOnSpawn: false);
            Pending.RemoveAt(0);
            _crushCounter = 0;

            if (Helper.IsHost())
                StorageSync.SendCraftUpdate(uid, GetStoragePool(), Pending);
        }
        else
        {
            _crushCounter++;
        }
    }
}
