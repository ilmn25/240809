using UnityEngine;

/// <summary>Base for passive NPCs (merchant, guide, questmaster, nomad). They
/// flee from attackers but fight back if cornered.</summary>
public abstract class PassiveNPCMachine : GroundMobMachine
{
    public override void OnStart()
    {
        base.OnStart();

        AddState(new MobIdle(600)); // lingers in place longer than the animals
        AddState(new MobRoam());
        AddState(new MobHit());
        AddState(new MobEscapeFight<MobAttackSwing>());
        AddState(new MobAttackSwing());
        AddState(new EquipSelectState());

        // Carries a blade to defend itself if attacked.
        Info.SetEquipment(new ItemSlot(ID.SteelSword));
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
