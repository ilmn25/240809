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

    public override void LateUpdate()
    {
        base.LateUpdate();
        if (GlowsWhenConverting && GlowLight != null && Info is StructureInfo)
            SetGlow(((ConverterInfo)Info).IsConverting());
    }
}
