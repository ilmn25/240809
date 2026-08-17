using System.Collections.Generic;

/// <summary>The kinds of liquid a pond can hold and a bucket can be filled with.</summary>
public enum LiquidType
{
    Water,
    Lava,
    Honey,
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
    };

    public static bool TryGetFilledBucket(LiquidType type, out ID filled)
        => FilledBucket.TryGetValue(type, out filled);
}
