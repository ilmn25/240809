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
        if (IsCurrentState<DefaultState>())
        {
            Dialogue.Target = new Dialogue { Text = "Hand me your relics — I'll melt them down to gold." };
            Dialogue.Show(true);
            Audio.PlaySFX(SfxID.Notification);
            SetState<InContainerState>();
        }
        else
        {
            SetState<DefaultState>();
        }
    }

    public override void OnUpdate()
    {
        UpdateFlee();
        Converter.Update();
    }

    public void OnDrawGizmos()
    {
        if (Camera.current == Camera.main)
            GetModule<GroundPathingModule>().DrawGizmos();
    }
}
