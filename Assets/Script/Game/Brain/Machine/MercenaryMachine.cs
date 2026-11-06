using UnityEngine;

public class MercenaryMachine : PassiveNPCMachine, IActionSecondaryInteract
{
    /// <summary>Hire price in copper (5 silver).</summary>
    public const int HireCost = 5 * Trade.CopperPerSilver;
    public const float CompanionDuration = 45f;

    public MercenaryTentMachine Tent;

    public static Info CreateInfo()
    {
        PassiveInfo info = PassiveInfo.CreateNpc();
        info.CharSprite = ID.Raider;
        return info;
    }

    public void OnActionSecondary(Info info)
    {
        BeginInteraction();

        PlayerInfo payer = info as PlayerInfo ?? Main.PlayerInfo;

        int carried = Trade.Count(payer.Storage);
        if (carried < HireCost)
        {
            Dialogue.ShowEvent($"\"I need {Trade.Format(HireCost)}. You only have {Trade.Format(carried)}.\"");
            return;
        }

        ShowMenu(payer);
    }

    /// <summary>Offers a short contract and hires on the spot.</summary>
    private void ShowMenu(PlayerInfo payer)
    {
        ShowDialogue(new Dialogue
        {
            Text = $"\"Need a hand? {Trade.Format(HireCost)} for a short contract.\"",
            OnOpen = () => CompleteHire(payer),
        });
    }

    private void CompleteHire(PlayerInfo payer)
    {
        if (Save.Inst == null || payer?.Storage == null) return;
        if (!Trade.TrySpend(payer.Storage, HireCost)) return;

        DelverMachine.Spawn(transform.position, ID.Raider, persist: false,
            expireAt: Time.time + CompanionDuration, startingWeapon: ID.SteelSword);

        Tent?.Consume();

        Info.Destroy();
        Unload();
    }
}
