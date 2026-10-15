using UnityEngine;

/// <summary>A friendly nomad of the travelling bandwagon. The bandwagon leader
/// (IsLeader) opens a shop that sells goods for specific resources; the rest just
/// cluster around the wagon and trade a line of dialogue. When spawned by a
/// caravan the nomad follows the wagon and leaves with it (see
/// GroundMobMachine.UpdateCaravanFollow).</summary>
public class NomadMachine : PassiveNPCMachine, IActionSecondaryInteract, ITrader
{
    /// <summary>True for the single shopkeeper of a visiting bandwagon.</summary>
    public bool IsLeader;

    /// <summary>The goods this leader sells, shown through the craft UI. Created fresh per
    /// nomad so trade state never bleeds between instances.</summary>
    public CraftInfo Menu { get; private set; }

    public TradeMode Mode => TradeMode.Shop;

    public static Info CreateInfo()
    {
        PassiveInfo info = PassiveInfo.CreateNpc();
        info.DistAttack = 2;
        info.DistDisengage = 40;
        info.CharSprite = ID.Nomad;
        return info;
    }

    public override void OnStart()
    {
        base.OnStart();

        AddState(new TradeState());

        Menu = new CraftInfo { id = ID.Nomad };
    }

    public void OnActionSecondary(Info info)
    {
        BeginInteraction();

        // The leader opens its shop the moment you interact; the rest just talk.
        if (IsLeader)
            ToggleStation<TradeState>("The road is long. Take a look at what we've gathered.");
        else
            ShowDialogue("We trade where the winds take us. Safe travels.");
    }
}
