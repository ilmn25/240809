using UnityEngine;

/// <summary>A wandering pilgrim who sells a short escort for coin, then moves on.</summary>
public class PilgrimMachine : RecruitableNPCMachine
{
    public const int HireCost = 3 * Trade.CopperPerSilver;

    public static Info CreateInfo()
    {
        PassiveInfo info = PassiveInfo.CreateNpc();
        info.CharSprite = ID.Nomad;
        return info;
    }

    public override string Announce => "A wandering pilgrim hails you, offering an escort...";
    protected override string Greeting => "\"Roads are bad tonight. My axe is yours for a fair price.\"";
    protected override string Backstory => "\"I've walked every road in the abyss. Mostly it's walking that kills you.\"";
    protected override string RecruitText => "\"A short contract, nothing more. Then I move on.\"";
    protected override int CostCoins => HireCost;
    protected override float StaySeconds => 150f;
}
