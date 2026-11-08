using UnityEngine;

/// <summary>A raider who has had enough. Trades meat for a place in the party and stays.</summary>
public class DeserterMachine : RecruitableNPCMachine
{
    public static Info CreateInfo()
    {
        PassiveInfo info = PassiveInfo.CreateNpc();
        info.CharSprite = ID.Raider;
        return info;
    }

    public override string Announce => "A deserter steps out of the brush, hands raised...";
    protected override string Greeting => "\"Easy. I'm done with the raiders. Looking for a way out of here.\"";
    protected override string Backstory => "\"They made me do things. I'd rather starve than go back.\"";
    protected override string RecruitText => "\"Take me along and I'll keep the wolves off your back.\"";
    protected override ID CostItem => ID.Meat;
    protected override int CostAmount => 3;
    protected override ID Weapon => ID.CrudeHatchet;
}
