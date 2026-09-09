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

public abstract class LootChestMachine : ChestMachine
{
    protected virtual string SpritePath => "Sprite/Chest";

    public override void OnSetup()
    {
        base.OnSetup();
        SpriteRenderer.sprite = Cache.LoadSprite(SpritePath);
    }

    protected static ContainerInfo CreateLootContainer(ID lootID)
    {
        Storage storage = new Storage(9);
        Loot.Gettable(lootID).AddToContainer(storage);
        return new ContainerInfo()
        {
            Health = 500,
            Loot = ID.Null,
            SfxHit = SfxID.HitStone,
            SfxDestroy = SfxID.HitStone,
            Storage = storage
        };
    }
}

public class RaiderCampChestMachine : LootChestMachine
{
    public static Info CreateInfo() => CreateLootContainer(ID.RaiderCampChest);
}

public class DungeonChestMachine : LootChestMachine
{
    public static Info CreateInfo() => CreateLootContainer(ID.DungeonChest);
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

public class CyclopsChestInfo : ContainerInfo
{
    public bool Unlocked;

    public override void OnDestroy(MobInfo info)
    {
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
        Loot.Gettable(ID.Chest).AddToContainer(storage);
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
        if (Helper.IsHost() && !ChestInfo.Unlocked)
            RaiseGuard();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        if (!Helper.IsHost() || ChestInfo.Unlocked) return;

        if (_guard != null && _guard.Destroyed)
        {
            _guard = null;
            ChestInfo.Unlocked = true;
            OnUnlocked();
        }
        else if (_guard == null || _guard.Machine == null)
        {
            RaiseGuard();
        }
    }

    private void RaiseGuard()
    {
        Vector3Int cell = Vector3Int.FloorToInt(transform.position) + new Vector3Int(1, 0, 0);
        Info spawned = Entity.Spawn(ID.Cyclops, cell);
        GuardModule.Attach(spawned, transform.position);
        _guard = spawned;
    }

    private void OnUnlocked()
    {
        Audio.PlaySFX(SfxID.Text);
        Particle.Create(transform.position, Particles.HitDust, true);
        Dialogue.ShowEvent("The guardian falls — the chest unlocks.");
    }
}