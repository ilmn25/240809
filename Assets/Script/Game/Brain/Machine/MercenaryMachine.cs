using System.Collections.Generic;
using UnityEngine;

public class MercenaryMachine : PassiveNPCMachine, IActionSecondaryInteract
{
    /// <summary>Hire price in copper (5 silver).</summary>
    public const int HireCost = 5 * Trade.CopperPerSilver;
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
        BeginInteraction();

        PlayerInfo payer = info as PlayerInfo;
        if (payer == null) payer = Main.PlayerInfo;
        if (payer?.Storage == null)
        {
            Dialogue.ShowEvent("The mercenary looks at you expectantly.");
            return;
        }

        int carried = Trade.Count(payer.Storage);
        if (carried < HireCost)
        {
            Dialogue.ShowEvent($"\"I need {Trade.Format(HireCost)}. You only have {Trade.Format(carried)}.\"");
            return;
        }

        ShowMenu(payer);
    }

    /// <summary>Offers to hire the mercenary or walk away.</summary>
    private void ShowMenu(PlayerInfo payer)
    {
        Dialogue hire = new Dialogue
        {
            Text = $"\"Pleasure doing business.\" The mercenary takes the coin and joins you for {CompanionDuration:0} seconds.",
            OnOpen = () => CompleteHire(payer),
        };

        ShowDialogue(new Dialogue
        {
            Text = $"\"Need a hand? {Trade.Format(HireCost)} for a short contract.\"",
            Next = new Dictionary<string, Dialogue>
            {
                [$"Hire for {Trade.Format(HireCost)}"] = hire,
                ["Leave"] = new Dialogue { Text = "\"Stay safe out there.\"" },
            },
        });
    }

    private void CompleteHire(PlayerInfo payer)
    {
        if (Save.Inst == null || payer?.Storage == null) return;
        if (!Trade.TrySpend(payer.Storage, HireCost)) return;

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
