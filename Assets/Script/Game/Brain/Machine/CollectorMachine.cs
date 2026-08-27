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

        Converter = new TraderInfo() { Storage = new Storage(9) };
        Converter.Machine = this;
        Converter.Initialize();

        AddState(new InContainerState() { Storage = Converter.Storage });
    }

    public void OnActionSecondary(Info info)
    {
        if (Info.Target != null) return;
        Audio.PlaySFX(SfxID.Notification);
        // The trader idles in MobIdle (not DefaultState), so toggle on
        // InContainerState instead of the chest's DefaultState pattern.
        if (IsCurrentState<InContainerState>())
            SetState<DefaultState>();
        else
            SetState<InContainerState>();
    }

    public override void OnUpdate()
    {
        // A caravan trader follows the wagon and leaves when it's gone.
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
}
