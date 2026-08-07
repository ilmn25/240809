using UnityEngine;

public class StructureMachine : EntityMachine, IActionPrimaryResource
{
    protected SpriteRenderer SpriteRenderer;
    protected Light GlowLight;

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
        // Any flammable structure gets the fire module automatically.
        if (Info.Flammable)
            AddModule(new FlammableModule());
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