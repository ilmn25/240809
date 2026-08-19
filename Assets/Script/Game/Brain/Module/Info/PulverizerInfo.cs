using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class PulverizerInfo : ConverterInfo
{
    private static readonly ID[] Ores =
    {
        ID.Copper,
        ID.Steel,
        ID.Slag,
        ID.Charcoal,
        ID.Fossil,
    };

    protected override IReadOnlyList<ID> GetOutputs(ID input)
    {
        return input is ID.Geode or ID.PetrifiedDelver ? new[] { Ores[Random.Range(0, Ores.Length)] } : null;
    }
}
