/// <summary>A travelling trader who melts scrap down into coin: right-click it to open
/// what it buys and click a piece to sell it.</summary>
public class CollectorMachine : PassiveNPCMachine, IActionSecondaryInteract, ITrader
{
    /// <summary>The scrap this collector buys, shown through the craft UI: five random pieces
    /// whose clicks sell them for coins worth them. Kept off the mob Info (which must stay a
    /// PassiveInfo).</summary>
    public CraftInfo Menu { get; private set; }

    /// <summary>Clicking a piece sells it for coins instead of crafting or buying it.</summary>
    public TradeMode Mode => TradeMode.Sell;

    public static Info CreateInfo() => PassiveInfo.CreateNpc();

    public override void OnStart()
    {
        base.OnStart();

        Menu = new CraftInfo { id = ID.Collector };
        AddState(new TradeState());
    }

    public void OnActionSecondary(Info info)
    {
        BeginInteraction();
        ToggleStation<TradeState>("Got any scrap to sell?", Cache.LoadSprite("Sprite/Collector"));
    }
}
