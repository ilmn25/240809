using UnityEngine;

/// <summary>A wandering tinkerer who turns salvage into ordnance. Bring scrap metal,
/// leave with a freshly built grenade.</summary>
public class ScrapTinkerMachine : RequestNPCMachine
{
    public static Info CreateInfo()
    {
        PassiveInfo info = PassiveInfo.CreateNpc();
        info.CharSprite = ID.Exchanger;
        return info;
    }

    public override string Announce => "A tinkerer rattles up with a cart of junk...";
    protected override string Greeting => "\"Got scrap metal? I can make it worth your while.\"";
    protected override string Thanks => "\"Pleasure doing business. Here — freshly made.\"";
    protected override ID RequestItem => ID.MetalChunks;
    protected override int RequestAmount => 3;
    protected override ID RewardItem => ID.Grenade;
}
