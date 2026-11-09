using UnityEngine;

/// <summary>A hermit who brews medicine from the abyss's fungi. Bring the right
/// mushrooms and they hand back bandages and a little coin.</summary>
public class ShrineHermitMachine : RequestNPCMachine
{
    public static Info CreateInfo()
    {
        PassiveInfo info = PassiveInfo.CreateNpc();
        info.CharSprite = ID.Nomad;
        return info;
    }

    public override string Announce => "A hooded hermit waits by a pile of stones...";
    protected override string Greeting => "\"You carry deathcaps? I could use them.\"";
    protected override string Thanks => "\"These will serve you better than they serve me.\"";
    protected override ID RequestItem => ID.Deathcap;
    protected override int RequestAmount => 2;
    protected override ID RewardItem => ID.Bandages;
    protected override int RewardAmount => 2;
    protected override int RewardCoins => 10;
}
