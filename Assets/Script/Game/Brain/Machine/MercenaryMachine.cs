using System.Collections.Generic;
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

        ShowMenu(payer);
    }

    public override void OnUpdate()
    {
        UpdateFlee();
    }

    /// <summary>Offers to hire the mercenary or walk away.</summary>
    private void ShowMenu(PlayerInfo payer)
    {
        Dialogue hire = new Dialogue
        {
            Text = $"\"Pleasure doing business.\" The mercenary takes the gold and joins you for {CompanionDuration:0} seconds.",
            OnOpen = () => CompleteHire(payer),
        };

        Dialogue.Target = new Dialogue
        {
            Text = $"\"Need a hand? {HireCost} gold for a short contract.\"",
            Next = new Dictionary<string, Dialogue>
            {
                [$"Hire for {HireCost} gold"] = hire,
                ["Leave"] = new Dialogue { Text = "\"Stay safe out there.\"" },
            },
        };
        Dialogue.Show(true);
        Audio.PlaySFX(SfxID.Notification);
    }

    private void CompleteHire(PlayerInfo payer)
    {
        if (Save.Inst == null || payer?.Storage == null) return;
        if (payer.Storage.Count(ID.Gold) < HireCost) return;

        payer.Storage.RemoveItem(ID.Gold, HireCost);

        PlayerInfo companion = (PlayerInfo)Entity.CreateInfo(ID.Delver, transform.position);
        companion.CharSprite = ID.Raider;
        companion.Storage.CreateAndAddItem(ID.SteelSword);
        if (Entity.SpawnFromInfo(companion, false) is DelverMachine delver)
            delver.ExpireAt = Time.time + CompanionDuration;

        Tent?.Consume();

        Info.Destroy();
        Unload();
    }
}
