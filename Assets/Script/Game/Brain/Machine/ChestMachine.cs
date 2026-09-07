using UnityEngine;

public abstract class ChestMachine : StructureMachine, IActionSecondaryInteract, IActionPrimaryResource
{ 
    public override void OnStart()
    { 
        base.OnStart();
        AddContainerState();
    }

    protected virtual void AddContainerState()
    {
        AddState(new InContainerState()
        {
            Storage = ((ContainerInfo)Info).Storage
        });
    }
    

    public virtual void OnActionSecondary(Info info)
    {
        if (IsCurrentState<DefaultState>())
            SetState<InContainerState>();
        else 
            SetState<DefaultState>();
    }
}

public class LootChestMachine : ChestMachine
{
    public static Info CreateInfo()
    {
        Storage storage = new Storage(9);
        Loot.Gettable(ID.Chest).AddToContainer(storage);
        return new ContainerInfo()
        {
            Health = 500,
            Loot = ID.PineTree,
            SfxHit = SfxID.HitStone,
            SfxDestroy = SfxID.HitStone,
            Storage = storage
        };
    }
}
public class BasicChestMachine : ChestMachine
{
    public static Info CreateInfo()
    {
        Storage storage = new Storage(9);
        return new ContainerInfo()
        {
            Health = 500,
            Loot = ID.PineTree,
            SfxHit = SfxID.HitStone,
            SfxDestroy = SfxID.HitStone,
            Storage = storage
        };
    }
}

/// <summary>A sealed chest guarded by a Cyclops boss (DST-style guardian). The
/// Cyclops stands guard beside the chest and the chest stays sealed while it
/// lives — slaying it drops the Cyclops's own loot and opens the chest for
/// good. Sealed chests spill nothing when smashed; only the guardian's death
/// opens them.</summary>
public class CyclopsChestInfo : ContainerInfo
{
    /// <summary>True once the guarding Cyclops has been slain — the chest is openable.</summary>
    public bool Unlocked;

    public override void OnDestroy(MobInfo info)
    {
        // Sealed chests yield nothing when destroyed — kill the guardian to open it.
        if (Unlocked)
            base.OnDestroy(info);
    }
}

public class CyclopsChestMachine : ChestMachine
{
    private Info _guard;

    public static Info CreateInfo()
    {
        Storage storage = new Storage(9);
        Loot.Gettable(ID.Chest).AddToContainer(storage); // boss-tier container loot
        return new CyclopsChestInfo()
        {
            Health = 500,
            Loot = ID.Null,
            SfxHit = SfxID.HitStone,
            SfxDestroy = SfxID.HitStone,
            Storage = storage,
        };
    }

    private CyclopsChestInfo ChestInfo => (CyclopsChestInfo)Info;

    public override void OnSetup()
    {
        base.OnSetup();
        // Looks like a normal chest until it's opened.
        SpriteRenderer.sprite = Cache.LoadSprite("Sprite/Chest");
    }

    public override void OnActionSecondary(Info info)
    {
        if (info is not PlayerInfo) return;
        if (!ChestInfo.Unlocked)
        {
            Dialogue.ShowEvent("Sealed by its guardian...");
            return;
        }
        base.OnActionSecondary(info);
    }

    public override void OnStart()
    {
        base.OnStart();
        // (Re)loaded while still sealed — put its Cyclops back on post.
        if (Helper.IsHost() && !ChestInfo.Unlocked)
            RaiseGuard();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        if (!Helper.IsHost() || ChestInfo.Unlocked) return;

        // The guardian fell — the seal breaks for good.
        if (_guard != null && _guard.Destroyed)
        {
            _guard = null;
            ChestInfo.Unlocked = true;
            OnUnlocked();
        }
        else if (_guard == null || _guard.Machine == null)
        {
            RaiseGuard(); // lost without dying (unloaded) — raise a fresh one
        }
    }

    /// <summary>Spawns the Cyclops just east of the chest and leashes it here.</summary>
    private void RaiseGuard()
    {
        Vector3Int cell = Vector3Int.FloorToInt(transform.position) + new Vector3Int(1, 0, 0);
        Info spawned = Entity.Spawn(ID.Cyclops, cell);
        if (spawned?.Machine is CyclopsMachine cyclops)
            cyclops.HomePosition = transform.position;
        _guard = spawned;
    }

    private void OnUnlocked()
    {
        Audio.PlaySFX(SfxID.Text);
        Particle.Create(transform.position, Particles.HitDust, true);
        Dialogue.ShowEvent("The guardian falls — the chest unlocks.");
    }
}