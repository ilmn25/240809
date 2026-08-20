using UnityEngine;

/// <summary>A travelling collector that trades relics for gold, like the
/// merchant but running a refinery. Interact to open its converter: drop a relic
/// into its storage and it smelts it down into gold.</summary>
public class CollectorMachine : PassiveNPCMachine, IActionSecondaryInteract
{
    /// <summary>The collector's converter — holds the relic storage and the
    /// relic-to-gold table. Kept off the mob Info (which must stay a PassiveInfo).</summary>
    public CollectorInfo Converter { get; private set; }

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

        Converter = new CollectorInfo() { Storage = new Storage(9) };
        Converter.Machine = this;
        Converter.Initialize();

        AddState(new InContainerState() { Storage = Converter.Storage });
    }

    public void OnActionSecondary(Info info)
    {
        if (Info.Target != null) return;
        Audio.PlaySFX(SfxID.Notification);
        // The collector idles in MobIdle (not DefaultState), so toggle on
        // InContainerState instead of the chest's DefaultState pattern.
        if (IsCurrentState<InContainerState>())
            SetState<DefaultState>();
        else
            SetState<InContainerState>();
    }

    public override void OnUpdate()
    {
        // A caravan collector follows the wagon and leaves when it's gone.
        if (Caravan != null)
        {
            // Don't interrupt an open converter or a hit reaction.
            if (IsCurrentState<InContainerState>() || IsCurrentState<MobHit>())
            {
                Converter.Update();
                return;
            }
            UpdateCaravanFollow();
            Converter.Update();
            return;
        }

        UpdateFlee();
        Converter.Update();
    }

    public void OnDrawGizmos()
    {
        if (Camera.current == Camera.main)
            GetModule<GroundPathingModule>().DrawGizmos();
    }
}
