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

        // Carries a blade to defend itself if attacked.
        Info.SetEquipment(new ItemSlot(ID.SteelSword));
    }

    /// <summary>True while focused on a real threat — the escort wagon doesn't count.</summary>
    protected bool IsEngaged =>
        Info.Target != null && (Caravan == null || Info.Target != Caravan.Info);

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

    /// <summary>Flee from the current threat, fighting back only when cornered;
    /// otherwise linger in place. Call from OnUpdate when not part of a caravan.</summary>
    protected void UpdateFlee()
    {
        if (!IsCurrentState<DefaultState>()) return;

        if (UpdateThreat()) return;

        SetState<MobIdle>(); // lingers in place (long idle from OnStart)
    }
}
