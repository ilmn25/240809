using System;
using System.Collections;
using UnityEngine;

public class StructureMachine : EntityMachine, IActionPrimaryResource
{
    protected SpriteRenderer SpriteRenderer;
    protected Light GlowLight;
    protected SpriteRenderer AttachmentRenderer;
    private bool _powered;

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
        GlowLight = transform.Find("Sprite/Glow")?.GetComponent<Light>();
        AttachmentRenderer = transform.Find("Sprite/Attachment")?.GetComponent<SpriteRenderer>();
        SetGlow(false);
    }

    public override void OnStart()
    { 
        AddModule(new SpriteOrbitModule()); 
        AddModule(new StructureSpriteCullModule());   
        // Pooled GameObjects are reused: reset glow/attachment left by the previous occupant.
        SetGlow(false);
        SetAttachment(null, false);
        SetGlow(Info is StructureInfo si && si.GlowOn);
    }

    public void SetGlow(bool on)
    {
        if (GlowLight != null)
            GlowLight.enabled = on;
    }

    protected void SetAttachment(Sprite sprite, bool shown = true)
    {
        if (AttachmentRenderer == null) return;
        AttachmentRenderer.sprite = sprite;
        AttachmentRenderer.gameObject.SetActive(shown);
    }

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