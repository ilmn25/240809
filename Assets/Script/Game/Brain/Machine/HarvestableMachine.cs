using UnityEngine;

/// <summary>Harvestable plants and decor. Can be harvested by attacking with any
/// tool (primary action) — no specific tool type required.</summary>
public class HarvestableMachine : StructureMachine
{
    public static Info CreateInfo()
    {
        return new HarvestableInfo();
    }

    public override void OnStart()
    {
        // Mark flammable BEFORE base.OnStart() so StructureMachine adds the
        // FlammableModule. Bush, Grass and the wooden Table are flammable; flowers are not.
        if (Info.id == ID.Bush || Info.id == ID.Grass || Info.id == ID.Table)
            Info.Flammable = true;

        base.OnStart();
        AddState(new StaticIdle(), true);
    }
}
