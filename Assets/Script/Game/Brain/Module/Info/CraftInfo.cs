using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[System.Serializable]
public class CraftInfo : SpriteStructureInfo
{
private static readonly Storage PlayerPool = CreateNoRefreshPool("Crafting", ID.CrudePickaxe, ID.CrudeHatchet, ID.CrudeMallet, ID.Toolbench, ID.Torch, ID.MulchBlock);
    private static readonly Dictionary<ID, PoolDef> PoolDefs = new Dictionary<ID, PoolDef>
    {
        { ID.Toolbench, new PoolDef(0, true, ID.Workbench, ID.Campfire, ID.Spear, ID.Hammer) },
        { ID.Campfire, new PoolDef(0, true, ID.Charcoal, ID.CookedMeat, ID.CookedChicken, ID.CookedDeathcap, ID.CrockPot) },
        { ID.CarpenterWorkbench, new PoolDef(0, true, ID.Bed, ID.Loom, ID.Sign, ID.Lamp, ID.Sawmill, ID.FieldStation, ID.Scarecrow) },
        { ID.Loom, new PoolDef(0, true, ID.Fabric, ID.Bandages) },
        { ID.Furnace, new PoolDef(0, true, ID.Slag, ID.Steel, ID.Copper) },
        { ID.Smelter, new PoolDef(0, true, ID.Glass) },
        { ID.MasonryWorkbench, new PoolDef(0, true, ID.Anvil, ID.Smelter, ID.Furnace, ID.Stonecutter, ID.OwlStatue) },
        { ID.Sawmill, new PoolDef(0, true, ID.Plank, ID.Stake, ID.Chest) },
        { ID.Stonecutter, new PoolDef(0, true, ID.Brick, ID.BrickBlock) },
        { ID.Workbench, new PoolDef(0, true, ID.StonePickaxe, ID.StoneHatchet, ID.MasonryWorkbench, ID.CarpenterWorkbench) },
        { ID.FieldStation, new PoolDef(0, true, ID.ImprovisedPlanter, ID.CornSeed, ID.PumpkinSeed) },
        { ID.Anvil, new PoolDef(0, true, ID.SteelSword, ID.MetalAxe, ID.Rapier, ID.Bucket, ID.Sprinkler, ID.Generator, ID.ArrowTrap, ID.OilBarrel, ID.Barrel) },
        { ID.Merchant, new PoolDef(0, false, ID.StonePickaxe, ID.StoneHatchet, ID.Hammer, ID.Spear, ID.SteelSword, ID.MetalAxe, ID.DiamondAxe, ID.Bed, ID.Lamp) },
        { ID.Nomad, new PoolDef(4, false, ID.DiamondAxe, ID.Rapier, ID.MetalAxe, ID.Spear, ID.Dagger, ID.Bandages, ID.Sulphur, ID.Casing, ID.Paper) },
    };
    
    public readonly List<ID> Pending = new List<ID>();
    public int Max = 10;
    public SfxID Sfx;

    [NonSerialized] private int _counter;
    [NonSerialized] private Storage _pool;

    public override void Initialize()
    {
        base.Initialize();
        operationType = OperationType.Cutting;
    }

    public override void Update()
    {
        if (Pending.Count == 0)
            return;

        if (Sfx != SfxID.Null)
            Audio.PlaySFX(Sfx);

        if (_counter == ItemRecipe.Dictionary[Pending[0]].Time)
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

    public Storage GetStoragePool()
    {
        ID poolID = id != ID.Null ? id : Loot;
        if (!PoolDefs.TryGetValue(poolID, out PoolDef def))
            return null;

        return def.Shared ? def.SharedStorage ??= Build(def, poolID)
                          : _pool ??= Build(def, poolID);
    }

    private static Storage Build(PoolDef def, ID poolID)
    {
        ID[] items = def.RandomCount > 0 ? PickRandom(def.Items, def.RandomCount) : def.Items;
        Storage storage = new NoRefreshStorage(items.Length);
        storage.Name = Helper.ToDisplayName(poolID);
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

    private static Storage CreatePool(params ID[] recipes)
    {
        Storage storage = new Storage(9);
        foreach (ID recipe in recipes)
            storage.CreateAndAddItem(recipe);

        return storage;
    }

    private static Storage CreatePool(string name, params ID[] recipes)
    {
        Storage storage = CreatePool(recipes);
        storage.Name = name;
        return storage;
    }

    private static Storage CreateNoRefreshPool(params ID[] recipes)
    {
        Storage storage = new NoRefreshStorage(9);
        foreach (ID recipe in recipes)
            storage.CreateAndAddItem(recipe);

        return storage;
    }

    private static Storage CreateNoRefreshPool(string name, params ID[] recipes)
    {
        Storage storage = CreateNoRefreshPool(recipes);
        storage.Name = name;
        return storage;
    }

    private class PoolDef
    {
        public readonly ID[] Items;
        public readonly int RandomCount;
        public readonly bool Shared;
        public Storage SharedStorage;

        public PoolDef(int randomCount, bool shared, params ID[] items)
        {
            Items = items;
            RandomCount = randomCount;
            Shared = shared;
        }
    }

}
