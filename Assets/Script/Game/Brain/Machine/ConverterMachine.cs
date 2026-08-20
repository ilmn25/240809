using UnityEngine;

public abstract class ConverterMachine : ChestMachine
{
    /// <summary>Machines whose light shines only while they are actively converting.</summary>
    protected virtual bool GlowsWhenConverting => false;

    public override void OnStart()
    {
        base.OnStart();

        ConverterInfo info = (ConverterInfo)Info;
        if (GlowsWhenConverting)
            SetGlow(info.IsConverting());

        StartEmitConvertParticles(info.IsConverting);
    }

    protected override void AddContainerState()
    {
        AddState(new ConverterContainerState()
        {
            Storage = ((ConverterInfo)Info).Storage,
            Converter = (ConverterInfo)Info,
        });
    }

    public override void LateUpdate()
    {
        base.LateUpdate();
        if (GlowsWhenConverting && GlowLight != null && Info is StructureInfo)
            SetGlow(((ConverterInfo)Info).IsConverting());
    }

    public override void OnActionSecondary(Info info)
    {
        if (IsCurrentState<DefaultState>())
        {
            if (((ConverterInfo)Info).IsConverting())
                return;
            SetState<InContainerState>();
        }
        else
            SetState<DefaultState>();
    }
}

public class CrockPotMachine : ConverterMachine
{
    public static Info CreateInfo()
    {
        return new CrockPotInfo()
        {
            Health = 300,
            Loot = ID.CrockPot,
            SfxHit = SfxID.HitMetal,
            SfxDestroy = SfxID.HitMetal,
            Storage = new Storage(9),
        };
    }
}

public class ConverterContainerState : InContainerState
{
    public ConverterInfo Converter;
    public override void OnExitState()
    {
        base.OnExitState();
        Converter.ResetTimer();
    }
}
