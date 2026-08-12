using UnityEngine;

/// <summary>Makes an entity flammable. Igniting applies a Burn status effect that
/// deals damage over time and shows "Burning" in the HUD. Fire spreads to nearby
/// flammable entities via FireRegistry and, for structures, consumes them.</summary>
public class FlammableModule : EntityModule
{
    /// <summary>How long (seconds) a structure burns before it is destroyed.</summary>
    public float BurnDuration = 25f;
    /// <summary>Damage dealt per second while burning.</summary>
    public float BurnDamagePerSecond = 4f;
    /// <summary>Radius within which this fire can ignite other flammable entities.</summary>
    public float SpreadRadius = 2.5f;
    /// <summary>Chance per spread tick to ignite a nearby flammable entity.</summary>
    public float SpreadChance = 0.12f;
    /// <summary>Seconds between spread attempts.</summary>
    public float SpreadInterval = 2.5f;
    /// <summary>Seconds between fire particle emissions.</summary>
    public float ParticleInterval = 0.5f;
    /// <summary>Seconds between smoke particle emissions.</summary>
    public float SmokeInterval = 1.2f;

    private float _burnTime;
    private float _spreadTimer;
    private float _particleTimer;
    private float _smokeTimer;

    private Info _info;
    public Info Info
    {
        get
        {
            if (_info == null)
                _info = EntityMachine.Info;
            return _info;
        }
    }

    public override void Initialize()
    {
        base.Initialize();
        if (Info.Flammable)
            FireRegistry.Register(this);
    }

    public override void Update()
    {
        if (!Helper.IsHost()) return;

        Info info = Info;
        if (info == null || info.Destroyed) return;
        if (info.FireLevel <= 0f) return;

        float dt = Helper.GetDeltaTime();

        info.FireLevel = Mathf.Min(1f, info.FireLevel + dt * 0.5f);

        _burnTime += dt;
        _spreadTimer += dt;
        _particleTimer += dt;
        _smokeTimer += dt;

        if (_particleTimer >= ParticleInterval)
        {
            _particleTimer = 0f;
            Particle.Create(info.position + new Vector3(0, 0.5f, 0), Particles.Fire, false);
        }

        if (_smokeTimer >= SmokeInterval)
        {
            _smokeTimer = 0f;
            Particle.Create(info.position + new Vector3(0, 0.8f, 0), Particles.Smoke, false);
        }

        if (_spreadTimer >= SpreadInterval)
        {
            _spreadTimer = 0f;
            FireRegistry.SpreadFrom(this);
        }

        // Structures are consumed by fire; living entities just burn until the
        // Burn status effect expires (or they die from the damage).
        if (info is StructureInfo && _burnTime >= BurnDuration)
            BurnOut(info);
    }

    /// <summary>Ignite this entity. Returns true if it started burning.</summary>
    public bool Ignite()
    {
        Info info = Info;
        if (info == null || info.Destroyed) return false;
        if (!info.Flammable) return false;
        if (info.FireLevel > 0f) return false;

        info.FireLevel = 0.15f;
        _burnTime = 0f;
        _spreadTimer = 0f;
        _particleTimer = 0f;
        _smokeTimer = 0f;

        // Route the burn damage through the status effect system so it ticks
        // uniformly (structures and living entities) and shows as "Burning".
        GetModule<StatusEffectModule>()?.Apply(new StatusEffect(
            ID.Burn, EffectType.Damage, BurnDuration, 1f, (int)BurnDamagePerSecond, name: "Burning"));

        Particle.Create(info.position + new Vector3(0, 0.5f, 0), Particles.Fire, false);
        return true;
    }

    /// <summary>Destroy a structure once it has burned out.</summary>
    private void BurnOut(Info info)
    {
        info.FireLevel = 0f;
        FireRegistry.Unregister(this);
        GetModule<StatusEffectModule>()?.Remove(ID.Burn);
        BurnOutcomeRegistry.Get(info.id)?.Apply(info.position);
        info.Destroy();
    }
}
