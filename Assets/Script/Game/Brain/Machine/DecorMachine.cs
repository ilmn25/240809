using UnityEngine;

public class DecorMachine : StructureMachine, IActionSecondaryInteract
{  
    public static Info CreateInfo()
    {
        return new Info();
    }
    public override void OnStart()
    {
        // Mark flammable BEFORE base.OnStart() so StructureMachine adds the
        // FlammableModule. Bush, Grass and the wooden Table are flammable; flowers are not.
        if (Info.id == ID.Bush || Info.id == ID.Grass || Info.id == ID.Table)
            Info.Flammable = true;

        base.OnStart();
        AddState(new StaticIdle(),true);  
    }

    /// <summary>Right-click to harvest the plant (like Don't Starve): drop its item
    /// and remove the plant from the world.</summary>
    public void OnActionSecondary(Info info)
    {
        // Only harvestable plants (Deathcap, Orchids) drop an item; plain decor
        // like Bush/Grass/Table just gets removed.
        if (Info.id == ID.Deathcap || Info.id == ID.Orchids)
            Entity.SpawnItem(Info.id, transform.position);

        Audio.PlaySFX(SfxID.Item);
        Info.Destroy();
    }
}