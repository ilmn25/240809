using UnityEngine;
 
public class FlammableModule : EntityModule
{
    /// <summary>How long (seconds) the object burns before it is destroyed.</summary>
    public float BurnDuration = 25f;
    /// <summary>Damage dealt to the structure per second while burning.</summary>
    public float BurnDamagePerSecond = 4f;
    /// <summary>Radius within which this fire can ignite other flammable objects.</summary>
    public float SpreadRadius = 2.5f;
    /// <summary>Chance per spread tick to ignite a nearby flammable object.</summary>
    public float SpreadChance = 0.12f;
    /// <summary>Seconds between spread attempts.</summary>
    public float SpreadInterval = 2.5f;
    /// <summary>Seconds between fire particle emissions.</summary>
    public float ParticleInterval = 0.5f;
    /// <summary>Seconds between smoke particle emissions.</summary>
    public float SmokeInterval = 1.2f;

    private float _burnTime;
    private float _damageTimer;
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
        _damageTimer += dt;

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

        if (_damageTimer >= 1f)
        {
            _damageTimer = 0f;
            ApplyBurnDamage(info);
        }

        if (_burnTime >= BurnDuration)
        {
            BurnOut(info);
        }
    }

    private void ApplyBurnDamage(Info info)
    {
        if (info is StructureInfo structure)
        {
            structure.Health -= BurnDamagePerSecond;
            if (structure.Health <= 0f)
            {
                BurnOut(info);
            }
        }
    }

    /// <summary>Ignite this object. Returns true if it started burning.</summary>
    public bool Ignite()
    {
        Info info = Info;
        if (info == null || info.Destroyed) return false;
        if (info.FireLevel > 0f) return false;

        info.FireLevel = 0.15f;
        _burnTime = 0f;
        _damageTimer = 0f;
        _spreadTimer = 0f;
        _particleTimer = 0f;
        _smokeTimer = 0f;
        Particle.Create(info.position + new Vector3(0, 0.5f, 0), Particles.Fire, false);
        return true;
    }

    /// <summary>Destroy the object once it has burned out.</summary>
    private void BurnOut(Info info)
    {
        info.FireLevel = 0f;
        FireRegistry.Unregister(this);

        BurnOutcomeRegistry.Get(info.id)?.Apply(info.position);

        info.Destroy();
    }
}
