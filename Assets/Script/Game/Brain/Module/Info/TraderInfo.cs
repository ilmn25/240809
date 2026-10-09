using System;
using System.Collections.Generic;

/// <summary>A converter that smelts materials down into coin. Drop a material
/// into its storage and it melts it down, paying out coins worth the item.</summary>
[Serializable]
public class TraderInfo : ConverterInfo
{
    /// <summary>Builds the converter a trader NPC carries. Unlike a structure this Info is
    /// never placed on the map, so it wires up its own id, machine and storage instead of
    /// running ContainerInfo.Initialize (which expects a real structure's "Sprite" child
    /// under Machine). The id is what gives the container window its title.</summary>
    public static TraderInfo Create(Machine machine, ID id)
    {
        TraderInfo trader = new TraderInfo
        {
            id = id,
            Machine = machine,
            Storage = new Storage(9),
        };
        trader.Storage.info = trader;
        return trader;
    }

    /// <summary>Scrap the refinery melts down, paid out at the item's worth in coins.</summary>
    private static readonly ID[] Accepted =
    {
        ID.Geode, ID.Fossil, ID.LeadChunks, ID.Lead, ID.Steel, ID.Slag, ID.Charcoal,
        ID.Sulphur, ID.Casing, ID.Gunpowder, ID.SteelSword, ID.Rapier, ID.DiamondAxe,
    };

    public static bool IsAccepted(ID input) => Array.IndexOf(Accepted, input) >= 0;

    protected override bool Accepts(ID input) => IsAccepted(input);

    protected override void PayOut(ID input)
    {
        foreach (KeyValuePair<ID, int> coin in Trade.Split(Trade.Worth(input)))
            SpawnOutput(coin.Key, coin.Value);
    }
}
