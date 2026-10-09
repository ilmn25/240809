using System.Collections.Generic;

/// <summary>A travelling money changer of the bandwagon: trades coins up a tier,
/// copper into silver and silver into gold. Interact to see its offers.</summary>
public class ExchangerMachine : PassiveNPCMachine, IActionSecondaryInteract
{
    public static Info CreateInfo()
    {
        return new PassiveInfo()
        {
            HealthMax = 50,
            SpeedGround = 5,
            SpeedAir = 6,
            DistRoam = 3,
            IsNPC = true,
        };
    }

    public void OnActionSecondary(Info info)
    {
        BeginInteraction();
        ShowDialogue(BuildMenu());
    }

    /// <summary>The money changer's offers: coins traded up a tier.</summary>
    private Dialogue BuildMenu()
    {
        Storage pocket = Main.PlayerInfo?.Storage;
        Dictionary<string, Dialogue> options = new Dictionary<string, Dialogue>();

        AddExchange(options, pocket, ID.Copper, ID.Silver, Trade.CopperPerSilver);
        AddExchange(options, pocket, ID.Silver, ID.Gold, Trade.SilverPerGold);

        options["Leave"] = new Dialogue { Text = "\"Safe travels.\"" };

        return new Dialogue
        {
            Text = "\"Copper for silver, silver for gold. What'll it be?\"",
            Sprite = Cache.LoadSprite("Sprite/Exchanger"),
            Next = options,
        };
    }

    private void AddExchange(Dictionary<string, Dialogue> options, Storage pocket, ID from, ID to, int batch)
    {
        int held = pocket?.Count(from) ?? 0;
        int trades = held / batch;
        if (trades <= 0) return;

        options[$"Trade {trades * batch} {from.ToString().ToLower()} for {trades} {to.ToString().ToLower()}"] = new Dialogue
        {
            Text = $"\"{trades} {to.ToString().ToLower()}, was it? A pleasure.\"",
            OnOpen = () =>
            {
                Trade.Exchange(pocket, from, to, batch);
                Audio.PlaySFX(SfxID.Item);
            },
        };
    }
}
