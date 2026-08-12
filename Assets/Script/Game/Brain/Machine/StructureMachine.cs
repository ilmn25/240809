using UnityEngine;

public class StructureMachine : EntityMachine, IActionPrimaryResource
{
    protected SpriteRenderer SpriteRenderer;
    protected Light GlowLight;
    private bool _powered;

    /// <summary>Whether a nearby generator is powering this structure.</summary>
    public bool Powered => _powered;

    public void SetPowered(bool powered)
    {
        if (_powered == powered) return;
        _powered = powered;
        OnPoweredChanged(powered);
    }

    public virtual void OnPoweredChanged(bool powered) { }

    public override void OnSetup()
    {
        SpriteRenderer = transform.Find("Sprite").GetComponent<SpriteRenderer>();
        SpriteRenderer.sprite = Cache.LoadSprite("Sprite/" + Info.id);
        GlowLight = transform.Find("Sprite/Glow").GetComponent<Light>();
        GlowLight.enabled = false; // default off; OnStart decides
    }

    public override void OnStart()
    { 
        AddModule(new SpriteOrbitModule()); 
        AddModule(new StructureSpriteCullModule());   
        if (GlowLight == null) return;
        GlowLight.enabled = Info is StructureInfo si && si.GlowOn;
    }

    /// <summary>Enable/disable this structure's glow light (furnace, lamp, ...).</summary>
    public void SetGlow(bool on)
    {
        if (GlowLight != null)
            GlowLight.enabled = on;
    }
} 