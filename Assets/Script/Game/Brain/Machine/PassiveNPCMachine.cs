using UnityEngine;

/// <summary>Base for passive NPCs (merchant, guide, questmaster, nomad). They
/// flee from attackers but fight back if cornered.</summary>
public abstract class PassiveNPCMachine : GroundMobMachine
{
    /// <summary>The wagon this NPC belongs to, set by CaravanMachine when it
    /// spawns its camp. While set, the NPC clusters around the wagon and leaves
    /// when the wagon flees or despawns.</summary>
    public CaravanMachine Caravan;

    public override void OnStart()
    {
        base.OnStart();

        AddState(new MobIdle(600)); // lingers in place longer than the animals
        AddState(new MobRoam());
        AddState(new MobChase());
        AddState(new MobHit());
        AddState(new MobEscapeFight<MobAttackSwing>());
        AddState(new MobAttackSwing());
        AddState(new EquipSelectState());
        AddState(new DialogueState(new Dialogue()));

        // Carries a blade to defend itself if attacked.
        Info.SetEquipment(new ItemSlot(ID.SteelSword));
    }

    public override void OnUpdate()
    {
        if (UpdateEscort()) return;

        if (!IsCurrentState<DefaultState>()) return;

        if (UpdateThreat()) return;

        SetState<MobIdle>(); // lingers in place (long idle from OnStart)
    }

    /// <summary>Follows the escort wagon, or handles a threat outranking it. Callers skip
    /// their usual idling while an interaction or hit reaction is running.</summary>
    private bool UpdateEscort()
    {
        if (Caravan == null) return false;

        if (IsInteracting || IsCurrentState<MobHit>()) return true;

        if (UpdateThreat()) return true;

        UpdateCaravanFollow(Caravan);
        return true;
    }

    /// <summary>True while focused on a real threat — the escort wagon doesn't count.</summary>
    protected bool IsEngaged =>
        Info.Target != null && (Caravan == null || Info.Target != Caravan.Info);

    protected void BeginInteraction()
    {
        if (IsEngaged) Info.CancelTarget();
    }

    protected void ShowDialogue(string text)
    {
        ShowDialogue(new Dialogue { Text = text });
    }

    protected void ShowDialogue(Dialogue dialogue)
    {
        DialogueState state = GetState<DialogueState>();
        state.Dialogue = dialogue;
        SetState<DialogueState>();
    }

    /// <summary>Shows a one-line greeting, typically over a station the caller opens right
    /// after. Shown directly rather than through DialogueState: entering the station would
    /// exit that state, which closes the line on the spot.</summary>
    protected void ShowGreeting(string text, Sprite portrait = null)
    {
        Dialogue.Target = new Dialogue { Text = text, Sprite = portrait };
        Dialogue.Show(true);
        Audio.PlaySFX(SfxID.Notification);
    }

    /// <summary>Opens the trader's station, or closes it when it is already open — the same
    /// toggle a crafting station uses. Re-opening an open station would only raise a second
    /// greeting, which hides the inventory GUI with nothing left to bring it back.</summary>
    protected void ToggleStation<T>(string greeting, Sprite portrait = null) where T : State
    {
        if (IsCurrentState<T>())
        {
            SetState<DefaultState>();
            return;
        }

        ShowGreeting(greeting, portrait);
        SetState<T>();
    }

    /// <summary>Handle a real threat: flee it, turning to fight back when cornered, and
    /// drop it once it gets away. Returns true while a threat is being handled so callers
    /// can skip their usual idling/escort behavior. The escort wagon never counts.</summary>
    protected bool UpdateThreat()
    {
        if (!IsEngaged) return false;

        if (Helper.SquaredDistance(Info.Target.position, transform.position) >
            Info.DistDisengage * Info.DistDisengage)
        {
            Info.CancelTarget(); // the threat got away — calm down
            return false;
        }

        SetState<MobEscapeFight<MobAttackSwing>>(); // flee, but fight back when cornered
        return true;
    }
}
