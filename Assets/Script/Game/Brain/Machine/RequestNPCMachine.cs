using System.Collections.Generic;

/// <summary>A traveller who asks for something and pays for it — the reverse of a
/// survivor: nobody joins the party, but handing over the requested goods earns a
/// reward. They wait where they appear, ask on interact, and move on once served or
/// ignored. Each subclass only fills in what they want, what they give and what they
/// say.</summary>
public abstract class RequestNPCMachine : TravellerNPCMachine
{
    /// <summary>Spoken as they take what they asked for.</summary>
    protected abstract string Thanks { get; }

    /// <summary>Item they ask for, or <see cref="ID.Null"/> for none.</summary>
    protected virtual ID RequestItem => ID.Null;
    protected virtual int RequestAmount => 1;
    /// <summary>Copper they ask for, or 0 for none.</summary>
    protected virtual int RequestCoins => 0;

    /// <summary>Item they hand over, or <see cref="ID.Null"/> for none.</summary>
    protected virtual ID RewardItem => ID.Null;
    protected virtual int RewardAmount => 1;
    /// <summary>Copper they hand over, or 0 for none.</summary>
    protected virtual int RewardCoins => 0;

    protected override EncounterDirector.Kind EncounterKind => EncounterDirector.Kind.Petitioner;

    private bool IsFree => RequestItem == ID.Null && RequestCoins <= 0;

    /// <summary>What they ask for, phrased for a dialogue line.</summary>
    private string RequestText => Format(RequestCoins, RequestItem, RequestAmount, "free");

    private static string Format(int coins, ID item, int amount, string empty)
    {
        if (coins > 0 && item != ID.Null)
            return $"{Trade.Format(coins)} and {amount} {Helper.ToDisplayName(item)}";
        if (coins > 0) return Trade.Format(coins);
        if (item != ID.Null) return $"{amount} {Helper.ToDisplayName(item)}";
        return empty;
    }

    private bool CanAfford(PlayerInfo payer)
    {
        if (IsFree) return true;
        if (payer?.Storage == null) return false;
        if (RequestCoins > 0 && Trade.Count(payer.Storage) < RequestCoins) return false;
        if (RequestItem != ID.Null && payer.Storage.Count(RequestItem) < RequestAmount) return false;
        return true;
    }

    /// <summary>Takes what they asked for out of the payer's bag. Only call after
    /// <see cref="CanAfford"/> passes.</summary>
    private void Pay(PlayerInfo payer)
    {
        if (payer?.Storage == null) return;
        if (RequestCoins > 0) Trade.TrySpend(payer.Storage, RequestCoins);
        if (RequestItem != ID.Null) payer.Storage.RemoveItem(RequestItem, RequestAmount);
    }

    /// <summary>Hands the reward over to the payer.</summary>
    private void Grant(PlayerInfo payer)
    {
        if (payer?.Storage == null) return;
        if (RewardCoins > 0) Trade.Pay(payer.Storage, RewardCoins);
        if (RewardItem != ID.Null) payer.Storage.CreateAndAddItem(RewardItem, RewardAmount);
    }

    /// <summary>A short menu: hand the goods over, or walk away.</summary>
    protected override void ShowMenu(PlayerInfo payer)
    {
        Dictionary<string, Dialogue> options = new Dictionary<string, Dialogue>();

        if (CanAfford(payer))
        {
            options[$"Hand over {RequestText}"] = new Dialogue
            {
                Text = Thanks,
                Sprite = Portrait,
                OnOpen = () => Exchange(payer),
            };
        }
        else
        {
            options["Hand over"] = Line($"Not enough {RequestText}.");
        }

        options["Not now"] = new Dialogue
        {
            Text = "\"Suit yourself.\"",
            Sprite = Portrait,
            OnOpen = Depart,
        };

        ShowRoot(options);
    }

    /// <summary>Takes the asking price, hands over the reward, and moves on.</summary>
    private void Exchange(PlayerInfo payer)
    {
        if (Save.Inst == null || payer?.Storage == null) return;
        if (!CanAfford(payer)) return;

        Pay(payer);
        Grant(payer);
        Depart();
    }
}