using UnityEngine;

/// <summary>A travelling trader that trades materials for gold, like the
/// merchant but running a refinery. Interact to open its converter: drop a
/// material into its storage and it smelts it down into gold.</summary>
public class CollectorMachine : PassiveNPCMachine, IActionSecondaryInteract
{
    /// <summary>The trader's converter — holds the material storage and the
    /// material-to-gold table. Kept off the mob Info (which must stay a PassiveInfo).</summary>
    public TraderInfo Converter { get; private set; }

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

    public override void OnStart()
    {
        base.OnStart();

        Converter = TraderInfo.Create(this, ID.Collector);
        AddState(new InContainerState() { Storage = Converter.Storage });
    }

    public void OnActionSecondary(Info info)
    {
        if (IsEngaged) return;

        Audio.PlaySFX(SfxID.Notification);

        // The trader idles in MobIdle rather than DefaultState, so toggle the container
        // state directly instead of following the chest's DefaultState pattern.
        if (IsCurrentState<InContainerState>())
            SetState<MobIdle>();
        else
            SetState<InContainerState>();
    }

    public override void OnUpdate()
    {
        // The converter keeps refining whatever else the trader is doing.
        Converter.Update();

        // A caravan trader follows the wagon and leaves when it's gone.
        if (Caravan != null)
        {
            // Don't interrupt an open converter or a hit reaction.
            if (IsCurrentState<InContainerState>() || IsCurrentState<MobHit>()) return;

            // A real threat outranks the escort wagon.
            if (UpdateThreat()) return;

            UpdateCaravanFollow(Caravan);
            return;
        }

        UpdateFlee();
    }
}
