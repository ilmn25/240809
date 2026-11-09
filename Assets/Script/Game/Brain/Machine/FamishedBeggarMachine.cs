using UnityEngine;

/// <summary>A gaunt survivor who trades the last of their coin for a meal. Bring food,
/// get paid; walk away and they move on.</summary>
public class FamishedBeggarMachine : RequestNPCMachine
{
    public static Info CreateInfo()
    {
        PassiveInfo info = PassiveInfo.CreateNpc();
        info.CharSprite = ID.BoundNPC;
        return info;
    }

    public override string Announce => "A gaunt figure shuffles up, clutching a few coins...";
    protected override string Greeting => "\"Please — anything to eat. I can pay, I swear it.\"";
    protected override string Thanks => "\"Bless you. Here — every coin I've got.\"";
    protected override ID RequestItem => ID.Meat;
    protected override int RequestAmount => 2;
    protected override int RewardCoins => 30; // 1 silver, 10 copper
}
