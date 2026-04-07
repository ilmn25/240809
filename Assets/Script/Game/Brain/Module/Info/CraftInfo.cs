using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[System.Serializable]
public class CraftInfo : SpriteStructureInfo
{
    private static readonly Storage PlayerPool = CreateNoRefreshPool("Crafting", ID.CrudePickaxe, ID.CrudeHatchet, ID.CrudeMallet, ID.Station, ID.Campfire);
    private static readonly Storage WorkbenchPool = CreateNoRefreshPool(ID.Spear, ID.StonePickaxe, ID.StoneHatchet, ID.Hammer);
    private static readonly Storage CampfirePool = CreateNoRefreshPool(ID.Charcoal, ID.CookedMeat);
    private static readonly Storage FurnacePool = CreateNoRefreshPool(ID.Slag, ID.Steel);
    private static readonly Storage SawmillPool = CreateNoRefreshPool(ID.Plank, ID.Stake, ID.Chest);
    private static readonly Storage StonecutterPool = CreateNoRefreshPool(ID.Brick, ID.BrickBlock);
    private static readonly Storage StationPool = CreatePool(ID.Blueprint, ID.Furnace, ID.Workbench, ID.Anvil, ID.Sawmill, ID.Stonecutter, ID.ImprovisedPlanter);
    private static readonly Storage AnvilPool = CreatePool(ID.SteelSword, ID.MetalAxe);

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
            ID.Workbench => WorkbenchPool,
            ID.Campfire => CampfirePool,
            ID.Furnace => FurnacePool,
            ID.Sawmill => SawmillPool,
            ID.Stonecutter => StonecutterPool,
            ID.Station => StationPool,
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
