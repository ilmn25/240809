using System.Collections.Generic;
using UnityEngine;

/// <summary>Item worth and the copper/silver/gold coin economy. Worth is a property of
/// the item (Item.Worth) and falls back to a raw-material value or the craft recipe.</summary>
public static class Trade
{
    public const int CopperPerSilver = 20;
    public const int SilverPerGold = 20;
    public const int CopperPerGold = CopperPerSilver * SilverPerGold;

    private static readonly ID[] Tiers = { ID.Gold, ID.Silver, ID.Copper };

    /// <summary>Value of things that aren't crafted from other items: gathered materials,
    /// relics and treasure. Seeds the derived worth of everything made from them.</summary>
    private static readonly Dictionary<ID, int> FixedWorth = new Dictionary<ID, int>
    {
        { ID.StoneBlock, 2 }, { ID.GrassBlock, 2 }, { ID.WoodBlock, 4 }, { ID.ForestBlock, 4 },
        { ID.Log, 4 }, { ID.MourningWood, 6 }, { ID.Sticks, 2 }, { ID.Sand, 2 }, { ID.Gravel, 2 },
        { ID.Mud, 2 }, { ID.Flint, 3 }, { ID.Ash, 2 }, { ID.Shell, 3 }, { ID.Wool, 5 },
        { ID.MetalChunks, 6 }, { ID.LeadChunks, 5 }, { ID.Meteorite, 8 }, { ID.Acorn, 2 },
        { ID.Cytoplasm, 4 }, { ID.SpiderWeb, 2 }, { ID.BirdShit, 2 }, { ID.Geode, 15 },
        { ID.Fossil, 20 }, { ID.Orchids, 2 }, { ID.Tulip, 2 }, { ID.Daisies, 2 },
        { ID.Deathcap, 4 }, { ID.OysterShrooms, 4 }, { ID.Toadstool, 4 }, { ID.Berries, 2 },
        { ID.Meat, 3 }, { ID.Egg, 3 }, { ID.Corn, 3 }, { ID.Pumpkin, 3 },

        { ID.PetrifiedDelver, 3 * CopperPerGold }, { ID.ThousandMenWedge, 5 * CopperPerGold },
        { ID.StarCompass, 8 * CopperPerGold }, { ID.UnheardBell, 8 * CopperPerGold },
        { ID.SunSphere, 10 * CopperPerGold }, { ID.BlazeReap, 15 * CopperPerGold },
        { ID.CradleOfBlood, 5 * CopperPerGold }, { ID.HornOfPlenty, 5 * CopperPerGold },
    };

    private static readonly Dictionary<ID, int> WorthCache = new Dictionary<ID, int>();

    public static int Worth(ID id)
    {
        if (WorthCache.TryGetValue(id, out int cached)) return cached;

        Item item = Item.GetItem(id);
        int worth = item?.Worth ?? 0;
        if (worth <= 0 && FixedWorth.TryGetValue(id, out int fixedValue)) worth = fixedValue;
        if (worth <= 0) worth = Derived(item);

        WorthCache[id] = worth;
        return worth;
    }

    public static int SellPrice(ID id) => Mathf.Max(1, Worth(id) / 2);

    public static bool IsCoin(ID id) => System.Array.IndexOf(Tiers, id) >= 0;

    /// <summary>The coins that make up the given copper amount, as item stacks.</summary>
    public static Dictionary<ID, int> Coins(int copper)
    {
        Dictionary<ID, int> coins = new Dictionary<ID, int>();
        foreach (KeyValuePair<ID, int> coin in Split(copper))
            coins[coin.Key] = coin.Value;
        return coins;
    }

    private static int Derived(Item item)
    {
        ItemRecipe recipe = item == null ? null : ItemRecipe.GetRecipe(item.ID);
        if (recipe == null) return 1;

        WorthCache[item.ID] = 0; // breaks recipe cycles

        int total = 0;
        foreach (KeyValuePair<ID, int> ingredient in recipe.Ingredients)
            total += Worth(ingredient.Key) * ingredient.Value;

        return Mathf.Max(1, total / Mathf.Max(1, recipe.Stack));
    }

    /// <summary>Total copper carried in the given storage.</summary>
    public static int Count(Storage storage)
    {
        if (storage?.List == null) return 0;

        return storage.Count(ID.Gold) * CopperPerGold
             + storage.Count(ID.Silver) * CopperPerSilver
             + storage.Count(ID.Copper);
    }

    /// <summary>Pays <paramref name="price"/> from the storage's coins, coining the change
    /// back. Fails (paying nothing) when the storage can't cover the price.</summary>
    public static bool TrySpend(Storage storage, int price)
    {
        if (price <= 0) return true;
        if (storage?.List == null) return false;

        int held = Count(storage);
        if (held < price) return false;

        foreach (ID coin in Tiers)
            storage.RemoveItem(coin, storage.Count(coin));

        Pay(storage, held - price);
        return true;
    }

    public static void Pay(Storage storage, int copper)
    {
        if (storage?.List == null) return;

        foreach (KeyValuePair<ID, int> coin in Split(copper))
            storage.CreateAndAddItem(coin.Key, coin.Value);
    }

    /// <summary>Splits a copper amount into coin stacks, largest tier first.</summary>
    public static IEnumerable<KeyValuePair<ID, int>> Split(int copper)
    {
        int gold = copper / CopperPerGold;
        copper %= CopperPerGold;
        int silver = copper / CopperPerSilver;
        copper %= CopperPerSilver;

        if (gold > 0) yield return new KeyValuePair<ID, int>(ID.Gold, gold);
        if (silver > 0) yield return new KeyValuePair<ID, int>(ID.Silver, silver);
        if (copper > 0) yield return new KeyValuePair<ID, int>(ID.Copper, copper);
    }

    public static string Format(int copper)
    {
        int gold = copper / CopperPerGold;
        copper %= CopperPerGold;
        int silver = copper / CopperPerSilver;
        copper %= CopperPerSilver;

        List<string> parts = new List<string>();
        if (gold > 0) parts.Add(gold + " gold");
        if (silver > 0) parts.Add(silver + " silver");
        if (copper > 0 || parts.Count == 0) parts.Add(copper + " copper");
        return string.Join(" ", parts);
    }
}
