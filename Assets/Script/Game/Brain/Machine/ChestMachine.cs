using UnityEngine;

public abstract class ChestMachine : StructureMachine, IActionSecondaryInteract, IActionPrimaryResource
{ 
    private bool _mimicTriggered;

    protected virtual float MimicChance => 0f;

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
        if (TryTriggerMimic()) return;

        if (IsCurrentState<DefaultState>())
            SetState<InContainerState>();
        else 
            SetState<DefaultState>();
    }

    private bool TryTriggerMimic()
    {
        if (_mimicTriggered || MimicChance <= 0f || !Helper.IsHost()) return false;
        if (UnityEngine.Random.value > MimicChance) return false;

        _mimicTriggered = true;
        if (Info is ContainerInfo container)
            container.Storage.List?.Clear();

        Entity.Spawn(ID.TreeMimic, Vector3Int.FloorToInt(transform.position));
        Audio.PlaySFX(SfxID.Noise);
        Dialogue.ShowEvent("The chest lunges — it's a mimic!");
        Info.Destroy();
        return true;
    }
}

public abstract class LootChestMachine : ChestMachine
{
    private const float BonusLootChance = 0.5f;

    protected override float MimicChance => 0.06f;

    protected static ContainerInfo CreateLootContainer(ID lootID)
    {
        Storage storage = new Storage(9);
        Loot.Gettable(lootID).AddToContainer(storage);
        if (UnityEngine.Random.value < BonusLootChance)
            Loot.Gettable(ID.Chest).AddOneRandom(storage);

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

public class ScavCampChestMachine : LootChestMachine
{
    public static Info CreateInfo() => CreateLootContainer(ID.ScavCampChest);
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

[System.Serializable]
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
        Info spawned = Entity.Spawn(ID.Cyclops, Vector3Int.FloorToInt(transform.position));
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