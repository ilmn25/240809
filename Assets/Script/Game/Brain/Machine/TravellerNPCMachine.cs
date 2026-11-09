using System.Collections.Generic;
using UnityEngine;

/// <summary>Shared behaviour for the travellers the <see cref="EncounterDirector"/> spawns:
/// they arrive, wait to be spoken to, defend themselves, and walk away when served,
/// refused or ignored. Subclasses only supply their lines, the encounter kind they report
/// back to, and the menu shown when spoken to.</summary>
public abstract class TravellerNPCMachine : PassiveNPCMachine, IActionSecondaryInteract, IEncounterAnnounce
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
    /// <summary>The director encounter this traveller belongs to.</summary>
    protected abstract EncounterDirector.Kind EncounterKind { get; }

    private float _leaveAt;

    protected Sprite Portrait => Cache.LoadSprite("Sprite/" + Info.CharSprite);

    public override void OnStart()
    {
        base.OnStart();

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
            if (Time.time >= _leaveAt) Depart();
            return;
        }

        // Defend the party from anything that wanders close.
        if (UpdateThreat()) return;

        PlayerInfo player = Main.PlayerInfo;
        if (player == null || player.Destroyed)
        {
            Depart();
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

    protected abstract void ShowMenu(PlayerInfo payer);

    /// <summary>Shows the greeting with the caller's choices.</summary>
    protected void ShowRoot(Dictionary<string, Dialogue> options)
    {
        ShowDialogue(new Dialogue
        {
            Text = Greeting,
            Sprite = Portrait,
            Next = options,
        });
    }

    /// <summary>A branch that only says something.</summary>
    protected Dialogue Line(string text) => new Dialogue { Text = text, Sprite = Portrait };

    /// <summary>Runs away from the player and despawns once far enough.</summary>
    private void WalkAway(PlayerInfo player)
    {
        _leaveAt = Time.time + FleeFallback;
        Info.SpeedGround = 6f;
        Info.Target = player;
        Info.PathingStatus = PathingStatus.Pending;
        SetState<MobFleeDespawn>();
    }

    /// <summary>Leaves the world and lets the director roll a fresh encounter of this kind.</summary>
    protected void Depart()
    {
        EncounterDirector.MarkLeft(EncounterKind);
        Info.Destroy();
        Unload();
    }
}