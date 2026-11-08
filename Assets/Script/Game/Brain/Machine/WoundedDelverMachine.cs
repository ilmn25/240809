using UnityEngine;

/// <summary>A wounded delver met on the roadside. Frail, but grateful enough to fight
/// for free and stay with the party for good.</summary>
public class WoundedDelverMachine : RecruitableNPCMachine
{
    public static Info CreateInfo()
    {
        PassiveInfo info = PassiveInfo.CreateNpc();
        info.CharSprite = ID.Delver;
        return info;
    }

    public override string Announce => "A wounded delver waves you down from the roadside...";
    protected override string Greeting => "\"Don't worry, I'm not one of them. Just banged up.\"";
    protected override string Backstory => "\"Fell into a ravine three days back. Climbed out on a dead man's rope.\"";
    protected override string RecruitText => "\"I owe you one. Point me at something and I'll swing.\"";
    protected override float HealthScale => 0.5f;
}
