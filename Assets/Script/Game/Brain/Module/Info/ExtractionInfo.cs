using System;
using System.Collections.Generic;

/// <summary>A converter that smelts relics down into gold. Drop a relic into its
/// storage and it melts it down, paying out gold after a short time.</summary>
[Serializable]
public class ExtractionInfo : ConverterInfo
{
    private static readonly Dictionary<ID, int> RelicValue = new Dictionary<ID, int>
    {
        { ID.PetrifiedDelver, 3 },
        { ID.ThousandMenWedge, 5 },
        { ID.StarCompass, 8 },
        { ID.UnheardBell, 8 },
        { ID.SunSphere, 10 },
        { ID.BlazeReap, 15 },
    };

    protected override IReadOnlyList<ID> GetOutputs(ID input)
    {
        return RelicValue.ContainsKey(input) ? new[] { ID.Gold } : null;
    }

    protected override int OutputAmount(ID input) => RelicValue[input];
}
