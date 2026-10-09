using System;
using System.Collections.Generic;

/// <summary>A converter that smelts relics down into coin. Drop a relic into its
/// storage and it melts it down, paying out coins worth the relic after a short time.</summary>
[Serializable]
public class ExtractionInfo : ConverterInfo
{
    /// <summary>Relics the extractor melts down, paid out at the relic's worth in coins.</summary>
    private static readonly ID[] Accepted =
    {
        ID.PetrifiedDelver, ID.ThousandMenWedge, ID.StarCompass, ID.UnheardBell,
        ID.SunSphere, ID.BlazeReap,
    };

    protected override bool Accepts(ID input) => Array.IndexOf(Accepted, input) >= 0;

    protected override void PayOut(ID input)
    {
        foreach (KeyValuePair<ID, int> coin in Trade.Split(Trade.Worth(input)))
            SpawnOutput(coin.Key, coin.Value);
    }
}
