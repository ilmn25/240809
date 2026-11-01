using UnityEngine;

/// <summary>Makes an entity flammable. Igniting applies a Burn status effect that
/// deals damage over time and shows "Burning" in the HUD. Fire spreads to nearby
/// flammable entities via FireRegistry and, for structures, consumes them.</summary>
public class FlammableModule : EntityModule
{
    /// <summary>How long (seconds) a structure burns before it is destroyed.</summary>
    public float BurnDuration = 8f;
    /// <summary>Damage dealt per second while burning.</summary>
    public float BurnDamagePerSecond = 1f;
    /// <summary>Radius within which this fire can ignite other flammable entities.</summary>
    public float SpreadRadius = 2.5f;
    /// <summary>Chance per spread tick to ignite a nearby flammable entity.</summary>
    public float SpreadChance = 0.08f;
    /// <summary>Seconds between spread attempts.</summary>
    public float SpreadInterval = 4f;
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
        // Flammability comes from the combustion registry (single source of truth).
        // Entities without a profile (e.g. mobs) keep their own default.
        CombustionProfile profile = CombustionRegistry.Get(Info.id);
        if (profile != null)
            Info.Flammable = profile.Flammable;
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
        _burnTime += dt;

        // Fire lasts exactly one BurnDuration — the same span as the Burn status
        // effect applied by Ignite. Structures are consumed by it; anything else
        // (players, mobs) just goes out, which also stops the fire particles.
        if (_burnTime >= BurnDuration)
        {
            if (info is StructureInfo)
                BurnOut(info);
            else
                Extinguish(info);
            return;
        }

        info.FireLevel = Mathf.Min(1f, info.FireLevel + dt * 0.5f);

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
    }

    /// <summary>Put out the fire without consuming the entity. FireRegistry
    /// registration is kept so nearby fires can ignite it again later.</summary>
    private void Extinguish(Info info)
    {
        info.FireLevel = 0f;
        ResetTimers();
    }

    private void ResetTimers()
    {
        _burnTime = 0f;
        _spreadTimer = 0f;
        _particleTimer = 0f;
        _smokeTimer = 0f;
    }

    /// <summary>Ignite this entity. Returns true if it started burning.</summary>
    public bool Ignite()
    {
        Info info = Info;
        if (info == null || info.Destroyed) return false;
        if (!info.Flammable) return false;
        if (info.FireLevel > 0f) return false;

        info.FireLevel = 0.15f;
        ResetTimers();

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
        Extinguish(info);
        FireRegistry.Unregister(this);
        GetModule<StatusEffectModule>()?.Remove(ID.Burn);

        // Registered entities burn to their profile output; anything else falls
        // back to dropping its loot, converting burnable materials to charcoal.
        CombustionProfile profile = CombustionRegistry.Get(info.id);
        if (profile != null)
            ApplyProfile(profile, info.position);
        else
            BurnUtil.DropBurnedLoot(info);

        info.Destroy();
    }

    /// <summary>Apply a profile's burn output: leave a structure, roll its loot
    /// table (converting burnable drops), and drop any fixed items.</summary>
    private static void ApplyProfile(CombustionProfile profile, Vector3 position)
    {
        if (profile.Spawns != ID.Null)
            Entity.Spawn(profile.Spawns, Vector3Int.FloorToInt(position));

        if (profile.DropsLoot != ID.Null && Loot.TryGet(profile.DropsLoot, out Loot table))
            table.SpawnBurned(position);

        foreach (BurnDrop drop in profile.Drops)
            Entity.SpawnItem(drop.ItemID, position, drop.Amount);
    }
}
