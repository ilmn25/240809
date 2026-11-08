using UnityEngine;

/// <summary>A half-starved stranger with nobody left. A scrap of food buys their loyalty.</summary>
public class StarvingStrangerMachine : RecruitableNPCMachine
{
    public static Info CreateInfo()
    {
        PassiveInfo info = PassiveInfo.CreateNpc();
        info.CharSprite = ID.BoundNPC;
        return info;
    }

    public override string Announce => "A tattered stranger stumbles up to you, half-starved...";
    protected override string Greeting => "\"Got any food? I'll pull my weight. I swear it.\"";
    protected override string Backstory => "\"My whole party is gone. Something with too many eyes took them.\"";
    protected override string RecruitText => "\"I'll stay as long as you'll have me. That's all I ask.\"";
    protected override ID CostItem => ID.Meat;
    protected override int CostAmount => 1;
    protected override float HealthScale => 0.8f;
    protected override ID Weapon => ID.StoneHatchet;
}
