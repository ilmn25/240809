using UnityEngine;

/// <summary>Base for passive NPCs (merchant, guide, questmaster, nomad). They
/// defend themselves against nearby enemies.</summary>
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
        AddState(new MobChase());
        AddState(new MobHit());
        AddState(new MobAttackSwing());
        AddState(new DialogueState(new Dialogue()));
        AddState(new EquipSelectState());

        // Carries a blade to defend itself if attacked.
        Info.SetEquipment(new ItemSlot(ID.SteelSword));
    }

    public override void OnUpdate()
    {
        if (IsInteracting || IsCurrentState<MobHit>()) return;

        if (UpdateThreat()) return;
        if (Caravan != null)
        {
            UpdateCaravanFollow(Caravan);
            return;
        }

        if (!IsCurrentState<DefaultState>()) return;
        SetState<MobIdle>(); // lingers in place (long idle from OnStart)
    }

    protected void BeginInteraction()
    {
        if (Info.Target is EnemyInfo) Info.CancelTarget();
    }

    protected void ShowDialogue(Dialogue dialogue)
    {
        DialogueState state = GetState<DialogueState>();
        state.DialogueData = dialogue;
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

    /// <summary>Handles a nearby threat and returns true while the NPC is busy with it.</summary>
    protected bool UpdateThreat()
    {
        if (Info.Target is EnemyInfo enemy &&
            (enemy.Destroyed ||
             Helper.SquaredDistance(enemy.position, transform.position) >
             Info.DistDisengage * Info.DistDisengage))
            Info.CancelTarget();

        if (Info.Target == null || Info.Target == Caravan?.Info)
        {
            Info.Target = EntityScan.FindNearest(transform.position, Info.DistAlert,
                info => info is EnemyInfo threat && !threat.Destroyed);
            if (Info.Target != null)
                Info.PathingStatus = PathingStatus.Pending;
        }

        if (Info.Target is not EnemyInfo) return false;

        if (Helper.SquaredDistance(Info.Target.position, transform.position) <
            Info.DistAttack * Info.DistAttack)
        {
            Info.AimPosition = Info.Target.position;
            SetState<MobAttackSwing>();
        }
        else
            SetState<MobChase>();
        return true;
    }
}
