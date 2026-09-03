using UnityEngine;

public class MercenaryMachine : PassiveNPCMachine, IActionSecondaryInteract
{
    public const int HireCost = 5;
    public const float CompanionDuration = 45f;

    public MercenaryTentMachine Tent;

    public static Info CreateInfo()
    {
        return new PassiveInfo()
        {
            HealthMax = 50,
            SpeedGround = 5,
            SpeedAir = 6,
            DistRoam = 3,
            CharSprite = ID.Raider,
            IsNPC = true,
        };
    }

    public void OnActionSecondary(Info info)
    {
        if (Info.Target != null) return;
        Hire(info);
    }

    public override void OnUpdate()
    {
        UpdateFlee();
    }

    private void Hire(Info info)
    {
        if (Save.Inst == null) return;

        PlayerInfo payer = info as PlayerInfo;
        if (payer == null) payer = Main.PlayerInfo;
        if (payer?.Storage == null)
        {
            Dialogue.ShowEvent("The mercenary looks at you expectantly.");
            return;
        }

        int gold = payer.Storage.Count(ID.Gold);
        if (gold < HireCost)
        {
            Dialogue.ShowEvent($"\"I need {HireCost} gold. You only have {gold}.\"");
            return;
        }

        payer.Storage.RemoveItem(ID.Gold, HireCost);

        PlayerInfo companion = (PlayerInfo)Entity.CreateInfo(ID.Delver, transform.position);
        companion.CharSprite = ID.Raider;
        companion.Storage.CreateAndAddItem(ID.SteelSword);
        if (Entity.SpawnFromInfo(companion, false) is DelverMachine delver)
            delver.ExpireAt = Time.time + CompanionDuration;

        Tent?.Consume();

        Dialogue.ShowEvent($"\"Pleasure doing business.\" {CompanionDuration:0}s left on their contract.");

        Info.Destroy();
        Unload();
    }
}
