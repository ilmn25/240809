using UnityEngine;

public class StructureMachine : EntityMachine, IActionPrimaryResource
{
    protected SpriteRenderer SpriteRenderer;
    protected Light GlowLight;

    /// <summary>Structures that are permanently lit (furnace, campfire, smelter, ...).</summary>
    protected virtual bool GlowsAlways => false;

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
        if (GlowsAlways && Info is StructureInfo litInfo)
        {
            litInfo.GlowOn = true; // keep in sync so save/network see it as lit
            GlowLight.enabled = true;
        }
        else
        {
            GlowLight.enabled = Info is StructureInfo si && si.GlowOn;
        }
    }

    /// <summary>Enable/disable this structure's glow light (furnace, lamp, ...).</summary>
    public void SetGlow(bool on)
    {
        if (GlowLight != null)
            GlowLight.enabled = on;
    }
} 