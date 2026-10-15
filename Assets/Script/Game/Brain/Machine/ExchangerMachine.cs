/// <summary>A travelling money changer of the bandwagon: right-click it to open the exchange
/// and click a coin to trade up a tier, twenty copper for a silver and twenty silver for a
/// gold.</summary>
public class ExchangerMachine : PassiveNPCMachine, IActionSecondaryInteract, ITrader
{
    /// <summary>The coin tiers this exchanger trades, shown through the craft UI: clicking one
    /// crafts it from a full stack of the tier below. Kept off the mob Info (which must stay a
    /// PassiveInfo).</summary>
    public CraftInfo Menu { get; private set; }

    /// <summary>The coins are ordinary recipes, so the window crafts rather than buys.</summary>
    public TradeMode Mode => TradeMode.Craft;

    public static Info CreateInfo() => PassiveInfo.CreateNpc();

    public override void OnStart()
    {
        base.OnStart();

        Menu = new CraftInfo { id = ID.Exchanger };
        AddState(new TradeState());
    }

    public void OnActionSecondary(Info info)
    {
        BeginInteraction();
        ToggleStation<TradeState>("Looking to exchange money?", Cache.LoadSprite("Sprite/Exchanger"));
    }
}
