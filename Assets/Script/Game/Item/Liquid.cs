using System.Collections.Generic;
using UnityEngine;

/// <summary>The kinds of liquid a pond can hold and a bucket can be filled with.</summary>
public enum LiquidType
{
    Water,
    Lava,
    Honey,
    Oil,
    None,
}

/// <summary>
/// Data-driven mapping between a liquid type and the item that results from
/// filling an empty bucket at a source of that liquid. To add a new liquid:
/// add an enum value, an ID, an item definition, a pond machine, and an entry
/// here — nothing else needs to change.
/// </summary>
public static class LiquidRegistry
{
    public const ID EmptyBucket = ID.Bucket;

    private static readonly Dictionary<LiquidType, ID> FilledBucket = new()
    {
        [LiquidType.Water] = ID.BucketOfWater,
        [LiquidType.Lava] = ID.BucketOfLava,
        [LiquidType.Honey] = ID.BucketOfHoney,
        [LiquidType.Oil] = ID.BucketOfOil,
    };

    public static bool TryGetFilledBucket(LiquidType type, out ID filled)
        => FilledBucket.TryGetValue(type, out filled);

    /// <summary>Reverse lookup: which liquid a filled bucket holds (BucketOfOil
    /// → Oil, etc.). Used when pouring a bucket's contents into a barrel.</summary>
    public static bool TryGetLiquidFromBucket(ID bucket, out LiquidType liquid)
    {
        foreach (var kv in FilledBucket)
        {
            if (kv.Value == bucket)
            {
                liquid = kv.Key;
                return true;
            }
        }
        liquid = default;
        return false;
    }

    /// <summary>Fill the player's held empty bucket from a liquid source. Returns
    /// true when a bucket was filled (the swing/interaction is consumed).</summary>
    public static bool TryFillBucket(MobInfo source, LiquidType liquid, Vector3 position)
    {
        if (!TryGetFilledBucket(liquid, out ID filled)) return false;
        if (source is not PlayerInfo player ||
            player.Storage?.List == null || player.Storage.List.Count == 0)
            return false;

        ItemSlot selected = player.Storage.GetSelected();
        if (selected.Stack <= 0 || selected.ID != EmptyBucket)
            return false;

        player.Storage.List[player.Storage.Key] = new ItemSlot(filled, 1);
        player.Storage.NotifyChanged();
        Particle.Create(position + Vector3.up * 0.5f, Particles.HitDust, false);
        return true;
    }
}
