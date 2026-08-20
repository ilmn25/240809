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

    /// <summary>Cluster around the wagon as a group, and leave once the wagon is
    /// gone. Call from OnUpdate when part of a caravan.</summary>
    protected void UpdateCaravanFollow()
    {
        if (Caravan == null || Caravan.Info == null || Caravan.Info.Destroyed)
        {
            Leave();
            return;
        }

        if (Vector3.Distance(Caravan.transform.position, transform.position) > Info.DistAttack)
        {
            Info.Target = Caravan.Info;
            Info.PathingStatus = PathingStatus.Pending;
            if (!IsCurrentState<MobChase>()) SetState<MobChase>();
        }
        else if (IsCurrentState<MobChase>())
            SetState<MobIdle>();
    }

    protected void Leave()
    {
        Info.Destroy();
        Unload();
    }

    /// <summary>Flee from the current threat, fighting back only when cornered;
    /// otherwise linger in place. Call from OnUpdate.</summary>
    protected void UpdateFlee()
    {
        if (!IsCurrentState<DefaultState>()) return;

        if (Info.Target != null)
        {
            if (Vector3.Distance(Info.Target.position, transform.position) > Info.DistDisengage)
                Info.CancelTarget(); // the threat got away — calm down
            else
                SetState<MobEscapeFight<MobAttackSwing>>(); // flee, but fight back when cornered
            return;
        }

        SetState<MobIdle>(); // lingers in place (long idle from OnStart)
    }
}
