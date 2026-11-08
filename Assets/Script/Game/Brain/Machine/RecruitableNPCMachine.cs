using System.Collections.Generic;
using UnityEngine;

/// <summary>Shared behaviour for the lone survivors met through a random encounter
/// (see <see cref="EncounterEvent"/>). Each survivor is its own entity — subclasses only
/// fill in how they look, what they say and what they ask for. They wait where they
/// appear and open a recruitment dialogue when spoken to; anything hostile that comes
/// close is fought off. Refused, or ignored for long enough, they walk away and the road
/// offers someone else another day.</summary>
public abstract class RecruitableNPCMachine : PassiveNPCMachine, IActionSecondaryInteract
{
    /// <summary>Real seconds they wait before moving on.</summary>
    private const float LeaveAfter = 90f;
    /// <summary>Distance they run to before despawning when they walk away.</summary>
    private const float LeaveDistance = 30f;
    /// <summary>Longest they spend walking off, in case the player follows them.</summary>
    private const float FleeFallback = 12f;

    /// <summary>One-line notice shown when the encounter spawns.</summary>
    public abstract string Announce { get; }
    protected abstract string Greeting { get; }
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
    protected virtual ID Weapon => ID.SteelSword;

    private float _leaveAt;

    private bool IsFree => CostItem == ID.Null && CostCoins <= 0;

    /// <summary>What they ask for, phrased for a dialogue line.</summary>
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

        int baseHealth = Info.HealthMax;
        if (HealthScale < 1f)
        {
            Info.HealthMax = Mathf.Max(1, Mathf.RoundToInt(baseHealth * HealthScale));
            Info.Health = Info.HealthMax;
        }
        _leaveAt = Time.time + LeaveAfter;

        AddState(new MobFleeDespawn(LeaveDistance));
    }

    public override void OnUpdate()
    {
        // Keep the offer open while the player reads the dialogue.
        if (IsInteracting)
        {
            _leaveAt = Time.time + LeaveAfter;
            return;
        }

        if (IsCurrentState<MobHit>() || IsCurrentState<MobAttackSwing>()) return;

        if (IsCurrentState<MobFleeDespawn>())
        {
            // The player may chase them; don't let the walk-off last forever.
            if (Time.time >= _leaveAt) Leave();
            return;
        }

        // Defend the party from anything that wanders close.
        if (UpdateThreat()) return;

        PlayerInfo player = Main.PlayerInfo;
        if (player == null || player.Destroyed)
        {
            Leave();
            return;
        }

        bool playerGone = Helper.SquaredDistance(player.position, transform.position) >
                          LeaveDistance * LeaveDistance;
        if (playerGone || Time.time >= _leaveAt)
        {
            WalkAway(player);
            return;
        }

        if (!IsCurrentState<DefaultState>()) return;
        SetState<MobIdle>(); // waits where they appeared, ready to be talked to
    }

    public void OnActionSecondary(Info info)
    {
        BeginInteraction();
        ShowMenu(info as PlayerInfo ?? Main.PlayerInfo);
    }

    private void ShowMenu(PlayerInfo payer)
    {
        Sprite portrait = Cache.LoadSprite("Sprite/" + Info.CharSprite);
        Dictionary<string, Dialogue> options = new Dictionary<string, Dialogue>();

        if (CanAfford(payer))
        {
            options[$"Recruit ({CostText})"] = new Dialogue
            {
                Text = RecruitText,
                Sprite = portrait,
                OnOpen = () => Recruit(payer),
            };
        }
        else
        {
            options["Recruit"] = new Dialogue
            {
                Text = $"\"{CostText}, friend. And you don't have it.\"",
                Sprite = portrait,
            };
        }

        options["Ask who they are"] = new Dialogue
        {
            Text = Backstory,
            Sprite = portrait,
        };

        options["Send them on their way"] = new Dialogue
        {
            Text = "\"Suit yourself. Safe travels.\"",
            Sprite = portrait,
            OnOpen = Decline,
        };

        ShowDialogue(new Dialogue
        {
            Text = Greeting,
            Sprite = portrait,
            Next = options,
        });
    }

    /// <summary>Takes the asking price and adds them to the party.</summary>
    private void Recruit(PlayerInfo payer)
    {
        if (Save.Inst == null || payer?.Storage == null) return;
        if (!CanAfford(payer)) return;

        Pay(payer);

        // A survivor on a contract leaves again when it runs out; one without
        // joins for good and is persisted with the rest of the party.
        DelverMachine delver = DelverMachine.Spawn(transform.position, Info.CharSprite,
            persist: StaySeconds <= 0f,
            expireAt: StaySeconds > 0f ? Time.time + StaySeconds : 0f,
            startingWeapon: Weapon);

        if (delver != null && HealthScale < 1f && delver.Info is PlayerInfo recruit)
        {
            int scaledMax = Mathf.Max(1, Mathf.RoundToInt(recruit.BaseHealthMax * HealthScale));
            recruit.BaseHealthMax = scaledMax;
            recruit.HealthMax = scaledMax;
            recruit.Health = scaledMax;
        }

        Leave();
    }

    private void Decline()
    {
        EncounterEvent.MarkLeft();
        Info.Destroy();
        Unload();
    }

    /// <summary>Runs away from the player and despawns once far enough.</summary>
    private void WalkAway(PlayerInfo player)
    {
        _leaveAt = Time.time + FleeFallback;
        Info.SpeedGround = 6f;
        Info.Target = player;
        Info.PathingStatus = PathingStatus.Pending;
        SetState<MobFleeDespawn>();
    }

    private void Leave()
    {
        EncounterEvent.MarkLeft();
        Info.Destroy();
        Unload();
    }
}
