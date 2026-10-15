/// <summary>What clicking a listed item does in a trader's window.</summary>
public enum TradeMode
{
    /// <summary>Craft it from its recipe, like a crafting station (the money changer).</summary>
    Craft,
    /// <summary>Buy it with coins (the merchant, the bandwagon leader).</summary>
    Shop,
    /// <summary>Sell it for coins worth it (the collector).</summary>
    Sell,
}

/// <summary>Implemented by the NPCs that open a trade window through the craft UI — the
/// merchant, the bandwagon leader, the money changer and the collector. Lets the state read
/// the menu and its mode without knowing the concrete machine type.</summary>
public interface ITrader
{
    /// <summary>The pool of items the window lists.</summary>
    CraftInfo Menu { get; }

    /// <summary>What clicking one of those items does.</summary>
    TradeMode Mode { get; }
}
