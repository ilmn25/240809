using System.Collections.Generic;
using UnityEngine;

/// <summary>A lone survivor met through a random encounter (see
/// <see cref="EncounterDirector"/>). Subclasses only fill in how they look, what they say
/// and what they ask for; recruiting them adds a real party member or a temporary
/// companion.</summary>
public abstract class RecruitableNPCMachine : TravellerNPCMachine
{
    protected abstract string Backstory { get; }
    /// <summary>Spoken as they agree to join.</summary>
    protected abstract string RecruitText { get; }

    /// <summary>Item they ask for, or <see cref="ID.Null"/> for none.</summary>
    protected virtual ID CostItem => ID.Null;
    protected virtual int CostAmount => 0;
    /// <summary>Coin price in copper, or 0 for none.</summary>
    protected virtual int CostCoins => 0;

    /// <summary>Fraction of the party member's normal health they join with.</summary>
    protected virtual float HealthScale => 1f;
    /// <summary>Contract length in real seconds; 0 means they join the party for good.</summary>
    protected virtual float StaySeconds => 0f;
    protected virtual ID Weapon => ID.CrudeHatchet;

    protected override EncounterDirector.Kind EncounterKind => EncounterDirector.Kind.Survivor;

    private bool IsFree => CostItem == ID.Null && CostCoins <= 0;

    private string CostText =>
        CostCoins > 0 ? Trade.Format(CostCoins)
        : CostItem != ID.Null ? $"{CostAmount} {Helper.ToDisplayName(CostItem)}"
        : "free";

    private bool CanAfford(PlayerInfo payer)
    {
        if (IsFree) return true;
        if (payer?.Storage == null) return false;
        if (CostCoins > 0) return Trade.Count(payer.Storage) >= CostCoins;
        return payer.Storage.Count(CostItem) >= CostAmount;
    }

    /// <summary>Takes their price out of the payer's bag. Only call after
    /// <see cref="CanAfford"/> passes.</summary>
    private void Pay(PlayerInfo payer)
    {
        if (payer?.Storage == null) return;
        if (CostCoins > 0) Trade.TrySpend(payer.Storage, CostCoins);
        if (CostItem != ID.Null) payer.Storage.RemoveItem(CostItem, CostAmount);
    }

    public override void OnStart()
    {
        base.OnStart();

        if (HealthScale >= 1f) return;
        Info.HealthMax = Mathf.Max(1, Mathf.RoundToInt(Info.HealthMax * HealthScale));
        Info.Health = Info.HealthMax;
    }

    protected override void ShowMenu(PlayerInfo payer)
    {
        Dictionary<string, Dialogue> options = new Dictionary<string, Dialogue>();
        string terms = StaySeconds > 0f ? $"{Helper.FormatDuration(StaySeconds)} contract" : "joins for good";

        if (CanAfford(payer))
        {
            options[$"Recruit ({CostText}, {terms})"] = new Dialogue
            {
                Text = RecruitText,
                Sprite = Portrait,
                OnOpen = () => Recruit(payer),
            };
        }
        else
        {
            options["Recruit"] = Line($"\"{CostText}, friend. And you don't have it.\"");
        }

        options["Ask who they are"] = Line(Backstory);
        options["Send them on their way"] = new Dialogue
        {
            Text = "\"Suit yourself. Safe travels.\"",
            Sprite = Portrait,
            OnOpen = Depart,
        };

        ShowRoot(options);
    }

    /// <summary>Takes the asking price and adds them to the party.</summary>
    private void Recruit(PlayerInfo payer)
    {
        if (Save.Inst == null || payer?.Storage == null) return;
        if (!CanAfford(payer)) return;

        Pay(payer);

        if (StaySeconds > 0f)
            HireCompanion();
        else
            JoinParty();

        Depart();
    }

    /// <summary>A short contract: a temporary ally that follows and fights but never
    /// loots, exactly like a hired mercenary. It leaves when the contract runs out.</summary>
    private void HireCompanion()
    {
        DelverMachine.Spawn(transform.position, Info.CharSprite,
            persist: false, expireAt: Time.time + StaySeconds, startingWeapon: Weapon);
    }

    /// <summary>Joins for good as a real party member — fully controllable, selectable
    /// when switching characters, and saved with the rest of the party.</summary>
    private void JoinParty()
    {
        PlayerInfo recruit = (PlayerInfo)Entity.CreateInfo(ID.Player, transform.position);
        recruit.CharSprite = Info.CharSprite;
        // Slots into the same stat block the other party members start with; the
        // weakened ones arrive frail. BaseHealthMax is taken from this on Initialize.
        if (HealthScale < 1f)
            recruit.HealthMax = Mathf.Max(1, Mathf.RoundToInt(recruit.HealthMax * HealthScale));
        recruit.Storage.CreateAndAddItem(Weapon);

        Save.Inst.players.Add(recruit);
        Entity.SpawnFromInfo(recruit, false);
    }
}