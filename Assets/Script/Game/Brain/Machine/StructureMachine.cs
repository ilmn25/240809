using System;
using System.Collections;
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

    /// <summary>While <paramref name="isConverting"/> is true, periodically emit smoke + fire particles (host only).</summary>
    protected void StartEmitConvertParticles(Func<bool> isConverting)
    {
        IEnumerator Enumerator()
        {
            while (gameObject.activeSelf)
            {
                yield return new WaitForSeconds(3);
                if (isConverting() && Helper.IsHost())
                {
                    Particle.Create(transform.position, Particles.Smoke, false);
                    Particle.Create(transform.position, Particles.Fire, false);
                }
            }
        }
        StartCoroutine(Enumerator());
    }
} 