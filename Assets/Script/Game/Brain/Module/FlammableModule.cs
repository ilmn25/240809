using UnityEngine;

/// <summary>Makes an entity flammable. Igniting applies the shared Burning status
/// effect, which deals the damage over time, shows "Burning" in the HUD, and draws
/// flames anchored to this entity (so fire follows a moving target). This module
/// owns only the burn lifetime, smoke and spread to nearby flammables.</summary>
public class FlammableModule : EntityModule
{
    /// <summary>Radius within which this fire can ignite other flammable entities.</summary>
    public float SpreadRadius = 2.5f;
    /// <summary>Chance per spread tick to ignite a nearby flammable entity.</summary>
    public float SpreadChance = 0.08f;
    /// <summary>Seconds between spread attempts.</summary>
    public float SpreadInterval = 4f;
    /// <summary>Seconds between smoke particle emissions.</summary>
    public float SmokeInterval = 1.2f;

    private float _burnTime;
    private float _spreadTimer;
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

        // Fire lasts exactly one Burning effect. Structures are consumed by it;
        // anything else (players, mobs) just goes out. The flames themselves are
        // drawn by the effect, so they stay glued to whatever is burning.
        if (_burnTime >= StatusEffects.Burning.Duration)
        {
            if (info is StructureInfo)
                BurnOut(info);
            else
                Extinguish(info);
            return;
        }

        info.FireLevel = Mathf.Min(1f, info.FireLevel + dt * 0.5f);

        _spreadTimer += dt;
        _smokeTimer += dt;

        if (_smokeTimer >= SmokeInterval)
        {
            _smokeTimer = 0f;
            Particle.CreateAttached(EntityMachine, new Vector3(0f, 0.8f, 0f), Particles.Smoke, false);
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
        GetModule<StatusEffectModule>()?.Remove(ID.Burn);
    }

    private void ResetTimers()
    {
        _burnTime = 0f;
        _spreadTimer = 0f;
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

        // Damage, HUD text and the following flames all live in the status effect
        // system, so burning ticks uniformly for structures and living entities.
        GetModule<StatusEffectModule>()?.Apply(StatusEffects.Burning);
        return true;
    }

    /// <summary>Destroy a structure once it has burned out.</summary>
    private void BurnOut(Info info)
    {
        Extinguish(info);
        FireRegistry.Unregister(this);

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
