using UnityEngine;

/// <summary>
/// Data-driven burn conversion helpers. Burnability lives on the item
/// definition (Item.Burnable / Item.BurnResult), so any burnable material —
/// whether it's a block, a dropped loot item, or a placed structure — converts
/// to its burn result (charcoal by default) when burned. This is what lets the
/// fire system "burn items" generically instead of hardcoding each entity.
/// </summary>
public static class BurnUtil
{
    /// <summary>Whether the item is marked combustible.</summary>
    public static bool IsBurnable(ID id)
    {
        Item item = Item.GetItem(id);
        return item != null && item.BurnResult != ID.Null;
    }

    /// <summary>What an item becomes when burned: its burn result if it is
    /// burnable, otherwise the item unchanged.</summary>
    public static ID BurnResultOf(ID id)
    {
        Item item = Item.GetItem(id);
        return item != null && item.BurnResult != ID.Null ? item.BurnResult : id;
    }

    /// <summary>Drop a burned structure's loot at its position, converting any
    /// burnable loot (wood, plants) to its burn result. Structures with no loot
    /// table fall back to yielding their own material if it is burnable.</summary>
    public static void DropBurnedLoot(Info info)
    {
        Vector3Int wp = Vector3Int.FloorToInt(info.position);

        if (info is StructureInfo si && si.Loot != ID.Null && Loot.TryGet(si.Loot, out Loot table))
        {
            table.SpawnBurned(info.position);
            return;
        }

        Item item = Item.GetItem(info.id);
        if (item != null && item.BurnResult != ID.Null)
            Entity.SpawnItem(item.BurnResult, wp, stackOnSpawn: false);
    }
}
