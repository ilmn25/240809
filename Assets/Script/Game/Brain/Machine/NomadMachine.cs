using UnityEngine;

/// <summary>A friendly nomad of the travelling bandwagon. Each nomad opens the
/// bandwagon's shop, which sells goods for specific resources. When spawned by a
/// caravan the nomad follows the wagon and leaves with it (see
/// GroundMobMachine.UpdateCaravanFollow).</summary>
public class NomadMachine : PassiveNPCMachine, IActionSecondaryInteract, ITrader
{
    /// <summary>The goods sold, shown through the craft UI. Created fresh per nomad so
    /// trade state never bleeds between instances.</summary>
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
        ToggleStation<TradeState>("The road is long. Take a look at what we've gathered.");
    }
}
