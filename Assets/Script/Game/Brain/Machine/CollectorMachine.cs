using System.Collections.Generic;
using UnityEngine;

/// <summary>A travelling trader running a refinery: melt scrap and valuables into
/// coin. Interact to see its offers.</summary>
public class CollectorMachine : PassiveNPCMachine, IActionSecondaryInteract
{
    /// <summary>The trader's converter — holds the scrap storage and pays out at
    /// the item's worth. Kept off the mob Info (which must stay a PassiveInfo).</summary>
    public TraderInfo Converter { get; private set; }

    public static Info CreateInfo()
    {
        return new PassiveInfo()
        {
            HealthMax = 50,
            SpeedGround = 5,
            SpeedAir = 6,
            DistRoam = 3,
            IsNPC = true,
        };
    }

    public override void OnStart()
    {
        base.OnStart();

        Converter = TraderInfo.Create(this, ID.Collector);
        AddState(new InContainerState() { Storage = Converter.Storage });
    }

    public void OnActionSecondary(Info info)
    {
        BeginInteraction();
        ShowDialogue(BuildMenu());
    }

    /// <summary>The trader's offer: melt scrap down into coin in the refinery.</summary>
    private Dialogue BuildMenu()
    {
        Dictionary<string, Dialogue> options = new Dictionary<string, Dialogue>
        {
            ["Open the refinery"] = new Dialogue
            {
                Text = "\"Drop your scrap in. I melt it down into coin.\"",
                OnOpen = () => SetState<InContainerState>(),
            },
            ["Leave"] = new Dialogue { Text = "\"Safe travels.\"" },
        };

        return new Dialogue
        {
            Text = "\"Coin for scrap. What'll it be?\"",
            Sprite = Cache.LoadSprite("Sprite/Collector"),
            Next = options,
        };
    }

    public override void OnUpdate()
    {
        // The converter keeps refining whatever else the trader is doing.
        Converter.Update();

        base.OnUpdate();
    }
}
