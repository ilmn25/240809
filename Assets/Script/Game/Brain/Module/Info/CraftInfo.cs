using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[System.Serializable]
public class CraftInfo : SpriteStructureInfo
{
private static readonly Storage PlayerPool = CreateNoRefreshPool(ID.CrudePickaxe, ID.CrudeHatchet, ID.CrudeMallet, ID.Spear, ID.Toolbench, ID.Torch, ID.MulchBlock, ID.Campfire);

    private static readonly ID[] MaterialWants =
    {
        ID.LeadChunks, ID.MetalChunks, ID.Meteorite, ID.Log, ID.Plank, ID.Sticks, ID.Wool,
        ID.Flint, ID.Gravel, ID.Sand, ID.Slag, ID.Charcoal, ID.Steel, ID.Lead, ID.Sulphur, ID.Casing,
        ID.Gunpowder, ID.Geode, ID.Fossil,
    };

    private static readonly ID[] RelicWants =
    {
        ID.PetrifiedDelver, ID.ThousandMenWedge, ID.StarCompass, ID.UnheardBell, ID.SunSphere,
        ID.BlazeReap, ID.CradleOfBlood, ID.HornOfPlenty,
    };

    private static readonly Dictionary<ID, PoolDef> PoolDefs = new Dictionary<ID, PoolDef>
    {
        { ID.Toolbench, new PoolDef(0, true, ID.StonePickaxe, ID.StoneHatchet,  ID.Rapier, ID.Sledgehammer, ID.Workbench, ID.Chest, ID.Furnace, ID.Lamp) },
        { ID.Campfire, new PoolDef(0, true, ID.Charcoal, ID.CookedMeat, ID.CookedChicken, ID.CookedDeathcap) },
        { ID.CarpenterWorkbench, new PoolDef(0, true, ID.Bed, ID.Loom, ID.Sign, ID.FieldStation, ID.Scarecrow, ID.Chair) },
        { ID.Loom, new PoolDef(0, true, ID.Fabric, ID.Bandages) },
        { ID.Furnace, new PoolDef(0, true, ID.Slag, ID.Lead) },
        { ID.CrockPot, new PoolDef(0, true, ID.CookedMeat, ID.CookedChicken, ID.CookedDeathcap) },
        { ID.Smelter, new PoolDef(0, true, ID.Glass, ID.Steel) },
        { ID.MasonryWorkbench, new PoolDef(0, true, ID.Smelter, ID.Stonecutter, ID.OwlStatue) },
        { ID.Sawmill, new PoolDef(0, true, ID.Plank, ID.Stake, ID.Chest, ID.WoodSpikeTrap, ID.Barrel, ID.OilBarrel) },
        { ID.Stonecutter, new PoolDef(0, true, ID.Brick, ID.BrickBlock) },
        { ID.Workbench, new PoolDef(0, true,   ID.Arrow, ID.ShortBow, ID.Anvil, ID.MasonryWorkbench, ID.CarpenterWorkbench, ID.Sawmill, ID.CrockPot) },
        { ID.FieldStation, new PoolDef(0, true, ID.ImprovisedPlanter, ID.CornSeed, ID.PumpkinSeed) },
        { ID.Anvil, new PoolDef(0, true, ID.SteelSword, ID.MetalAxe, ID.LongBow, ID.Bucket, ID.Sprinkler, ID.Generator, ID.MetalSpikeTrap, ID.CompoundBow, ID.Crossbow) },
        { ID.Merchant, new PoolDef(0, false, MaterialWants, ID.StonePickaxe, ID.StoneHatchet, ID.Sledgehammer, ID.Spear, ID.SteelSword, ID.MetalAxe, ID.DiamondAxe, ID.Bed, ID.Lamp) },
        { ID.Nomad, new PoolDef(4, false, RelicWants, ID.DiamondAxe, ID.Rapier, ID.MetalAxe, ID.Spear, ID.Dagger, ID.Bandages, ID.Sulphur, ID.Casing, ID.Paper) },

        { ID.Exchanger, new PoolDef(0, false, ID.Silver, ID.Gold) },
        { ID.Collector, new PoolDef(5, false, ID.Slag, ID.Charcoal, ID.LeadChunks, ID.Gravel, ID.Sand, ID.Flint, ID.Shell, ID.Mud, ID.Ash, ID.BirdShit, ID.SpiderWeb, ID.Sticks, ID.Cytoplasm, ID.Wool, ID.Cotton) },
    };

    public readonly List<ID> Pending = new List<ID>();

    [NonSerialized] private int _counter;
    [NonSerialized] private Storage _pool;

    public override void Initialize()
    {
        base.Initialize();
        operationType = OperationType.Cutting;
    }

    // Don't let the station be lifted away mid-craft, or the queued item is lost.
    protected override bool CanDismantleNow => Pending.Count == 0;
    protected override string DismantleBlockedMessage => "I should let it finish first.";

    public override void Update()
    {
        if (Pending.Count == 0)
            return;

        // Drop queued items that no longer have a recipe, so a stale queue can't stall.
        ItemRecipe recipe = ItemRecipe.GetRecipe(Pending[0]);
        if (recipe == null)
        {
            Pending.RemoveAt(0);
            return;
        }

        if (_counter == recipe.Time)
        {
            Entity.SpawnItem(Pending[0], Machine.transform.position + OutputOffset(), stackOnSpawn: false);
            Tutorial.OnCraft(Pending[0]);
            Pending.RemoveAt(0);
            _counter = 0;

            // Keep clients' pending queue (and crafting-machine glow) in sync
            // when a craft finishes, not just when a recipe is queued.
            if (Helper.IsHost())
                StorageSync.SendCraftUpdate(uid, GetStoragePool(), Pending);
        }
        else
        {
            _counter++;
        }
    }

    public bool IsConverting()
    {
        return Pending.Count > 0;
    }

    public static CraftInfo CreateStructureInfo(ID structureId, float health, SfxID sfxHit, SfxID sfxDestroy)
    {
        return new CraftInfo()
        {
            Health = health,
            Loot = structureId,
            SfxHit = sfxHit,
            SfxDestroy = sfxDestroy,
        };
    }

    /// <summary>Items this shop will buy from the player, paying their sell price.</summary>
    public bool WantsItem(ID itemID)
    {
        ID poolID = id != ID.Null ? id : Loot;
        if (!PoolDefs.TryGetValue(poolID, out PoolDef def) || def.Wants == null) return false;
        return System.Array.IndexOf(def.Wants, itemID) >= 0;
    }

    public Storage GetStoragePool()
    {
        ID poolID = id != ID.Null ? id : Loot;
        if (!PoolDefs.TryGetValue(poolID, out PoolDef def))
            return null;

        return def.Shared ? def.SharedStorage ??= Build(def)
                          : _pool ??= Build(def);
    }

    private static Storage Build(PoolDef def)
    {
        ID[] items = def.RandomCount > 0 ? PickRandom(def.Items, def.RandomCount) : def.Items;
        Storage storage = new NoRefreshStorage(items.Length);
        foreach (ID item in items)
            storage.CreateAndAddItem(item);

        return storage;
    }

    private static ID[] PickRandom(ID[] source, int count)
    {
        count = Mathf.Min(count, source.Length);
        ID[] copy = (ID[])source.Clone();
        for (int i = 0; i < count; i++)
        {
            int j = Random.Range(i, copy.Length);
            (copy[i], copy[j]) = (copy[j], copy[i]);
        }

        return copy[..count];
    }

    public static Storage GetPlayerPool()
    {
        return PlayerPool;
    }

    private static Storage CreateNoRefreshPool(params ID[] recipes)
    {
        Storage storage = new NoRefreshStorage(9);
        foreach (ID recipe in recipes)
            storage.CreateAndAddItem(recipe);

        return storage;
    }

    private class PoolDef
    {
        public readonly ID[] Items;
        public readonly int RandomCount;
        public readonly bool Shared;
        public readonly ID[] Wants;
        public Storage SharedStorage;

        public PoolDef(int randomCount, bool shared, ID[] wants, params ID[] items)
        {
            Items = items;
            RandomCount = randomCount;
            Shared = shared;
            Wants = wants;
        }

        public PoolDef(int randomCount, bool shared, params ID[] items)
            : this(randomCount, shared, null, items) { }
    }

}
