using UnityEngine;

/// <summary>A travelling merchant that lives at the Old Radio. It flees from
/// attackers (fighting back when cornered); interact with it to open its shop
/// (the craft UI with a fixed inventory of goods).</summary>
public class MerchantMachine : PassiveNPCMachine, IActionSecondaryInteract, ITrader
{
    /// <summary>The goods this merchant sells, shown through the craft UI. Created fresh
    /// per merchant so trade state never bleeds between instances.</summary>
    public CraftInfo Menu { get; private set; }

    public TradeMode Mode => TradeMode.Shop;

    public static Info CreateInfo() => PassiveInfo.CreateNpc();

    public override void OnStart()
    {
        base.OnStart();

        AddState(new TradeState());

        Menu = new CraftInfo { id = ID.Merchant };
    }

    public void OnActionSecondary(Info info)
    {
        BeginInteraction();
        ToggleStation<TradeState>("I've been tracking this signal for ages...");
    }

    public void OnDrawGizmos()
    {
        if (Camera.current == Camera.main)
            GetModule<GroundPathingModule>().DrawGizmos();
    }
}
