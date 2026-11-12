using UnityEngine;

/// <summary>A cultist who wants out of the congregation. Fights on a short contract,
/// and knows the rites better than most.</summary>
public class RenegadeCultistMachine : RecruitableNPCMachine
{
    public static Info CreateInfo()
    {
        PassiveInfo info = PassiveInfo.CreateNpc();
        info.CharSprite = ID.Cultist;
        return info;
    }

    public override string Announce => "A cultist turns away from the others, looking for a way out...";
    protected override string Greeting => "\"The congregation is... not what I thought. Can I come with you?\"";
    protected override string Backstory => "\"I only joined for the soup. Then they started singing.\"";
    protected override string RecruitText => "\"I know their rites. I can be useful. Please.\"";
    protected override float HealthScale => 0.7f;
    protected override float StaySeconds => 180f;
    protected override ID Weapon => ID.CrudeHatchet;
}
