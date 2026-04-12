using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[System.Serializable]
public class CraftInfo : SpriteStructureInfo
{
    private static readonly Storage PlayerPool = CreateNoRefreshPool("Crafting", ID.CrudePickaxe, ID.CrudeHatchet, ID.CrudeMallet, ID.Workbench, ID.Campfire, ID.MulchBlock);
    private static readonly Storage WoodenToolbenchPool = CreateNoRefreshPool(Helper.ToDisplayName(ID.WoodenToolbench), ID.Spear, ID.StonePickaxe, ID.StoneHatchet, ID.Hammer);
    private static readonly Storage CampfirePool = CreateNoRefreshPool(Helper.ToDisplayName(ID.Campfire), ID.Charcoal, ID.CookedMeat, ID.CookedChicken);
    private static readonly Storage CarpenterPool = CreateNoRefreshPool(Helper.ToDisplayName(ID.CarpenterWorkbench), ID.Bed, ID.Loom, ID.Sign);
    private static readonly Storage LoomPool = CreateNoRefreshPool(Helper.ToDisplayName(ID.Loom), ID.Fabric);
    private static readonly Storage FurnacePool = CreateNoRefreshPool(Helper.ToDisplayName(ID.Furnace), ID.Slag, ID.Steel, ID.Copper);
    private static readonly Storage SmelterPool = CreateNoRefreshPool(Helper.ToDisplayName(ID.Smelter), ID.Glass);
    private static readonly Storage MasonryPool = CreateNoRefreshPool(Helper.ToDisplayName(ID.MasonryWorkbench), ID.Anvil, ID.Smelter);
    private static readonly Storage SawmillPool = CreateNoRefreshPool(Helper.ToDisplayName(ID.Sawmill), ID.Plank, ID.Stake, ID.Chest);
    private static readonly Storage StonecutterPool = CreateNoRefreshPool(Helper.ToDisplayName(ID.Stonecutter), ID.Brick, ID.BrickBlock);
    private static readonly Storage WorkbenchPool = CreateNoRefreshPool(Helper.ToDisplayName(ID.Workbench), ID.Chalk, ID.Furnace, ID.MasonryWorkbench, ID.WoodenToolbench, ID.CarpenterWorkbench, ID.Sawmill, ID.Stonecutter, ID.FieldStation);
    private static readonly Storage FieldStationPool = CreateNoRefreshPool(Helper.ToDisplayName(ID.FieldStation), ID.ImprovisedPlanter);
    private static readonly Storage AnvilPool = CreateNoRefreshPool(Helper.ToDisplayName(ID.Anvil), ID.SteelSword, ID.MetalAxe);

    public readonly List<ID> Pending = new List<ID>();
    public int Max = 10;
    public SfxID Sfx;

    [NonSerialized] private int _counter;

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
            Vector3 offset = new Vector3(
                Random.value > 0.5f ? 0.65f : -0.65f,
                1.8f,
                Random.value > 0.5f ? 0.65f : -0.65f);

            Entity.SpawnItem(Pending[0], Machine.transform.position + offset, stackOnSpawn: false);
            Pending.RemoveAt(0);
            _counter = 0;
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
        return GetSharedPool(poolID);
    }

    public static Storage GetPlayerPool()
    {
        return PlayerPool;
    }

    private static Storage GetSharedPool(ID structureId)
    {
        return structureId switch
        {
            ID.WoodenToolbench => WoodenToolbenchPool,
            ID.CarpenterWorkbench => CarpenterPool,
            ID.Loom => LoomPool,
            ID.Campfire => CampfirePool,
            ID.Furnace => FurnacePool,
            ID.Smelter => SmelterPool,
            ID.MasonryWorkbench => MasonryPool,
            ID.Sawmill => SawmillPool,
            ID.Stonecutter => StonecutterPool,
            ID.Workbench => WorkbenchPool,
            ID.FieldStation => FieldStationPool,
            ID.Anvil => AnvilPool,
            _ => null,
        };
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

}
