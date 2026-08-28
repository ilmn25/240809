using System;
using System.Collections.Generic;

/// <summary>A converter that smelts materials down into gold. Drop a material
/// into its storage and it melts it down, paying out gold after a short time.</summary>
[Serializable]
public class TraderInfo : ConverterInfo
{
    private static readonly Dictionary<ID, int> GoldValue = new Dictionary<ID, int>
    {
        { ID.Geode, 2 },
        { ID.Fossil, 4 },
        { ID.Copper, 1 },
        { ID.Steel, 3 },
        { ID.Slag, 1 },
        { ID.Charcoal, 1 },
        { ID.Sulphur, 2 },
        { ID.Casing, 2 },
        { ID.Gunpowder, 3 },
        { ID.SteelSword, 5 },
        { ID.Rapier, 5 },
        { ID.DiamondAxe, 8 },
    };

    protected override IReadOnlyList<ID> GetOutputs(ID input)
    {
        return GoldValue.ContainsKey(input) ? new[] { ID.Gold } : null;
    }

    protected override int OutputAmount(ID input) => GoldValue[input];
}
