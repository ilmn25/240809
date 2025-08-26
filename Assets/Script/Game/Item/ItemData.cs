using System.Collections.Generic;
using UnityEngine;

public enum ItemRarity { Common, Rare, Epic, Legendary }
public enum ItemType { Tool, Armor, Accessory, Block, Structure, Material}
public enum ItemGesture { Swing, Poke, Cast, Shoot} 

public partial class Item
{
    public static void Initialize()
    {
        Loot loot;

        // Blocks
        AddBlockDefinition(ID.BrickBlock, 150, 1, SfxID.HitMetal, materials: new Dictionary<ID, int> { { ID.Gravel, 3 } });
        loot = Loot.CreateTable(ID.BrickBlock);
        loot.Add(1, 3, ID.Gravel);
        loot.Add(0.5f, 1, ID.Slag);
        loot.Add(0.5f, 1, ID.Brick);
        AddBlockDefinition(ID.MarbleBlock, 200, 1, SfxID.HitMetal, materials: new Dictionary<ID, int> { { ID.StoneBlock, 1 }, { ID.BrickBlock, 1 } }, craftStack: 2);
        AddBlockDefinition(ID.DirtBlock, 80, 1, SfxID.HitStone);
        loot = Loot.CreateTable(ID.DirtBlock);
        loot.Add(1, 3, ID.Gravel);
        loot.Add(0.5f, 1, ID.Flint);
        loot.Add(0.5f, 1, ID.Sticks);
        AddBlockDefinition(ID.SandBlock, 40, 1, SfxID.HitSand, materials: new Dictionary<ID, int> { { ID.StoneBlock, 1 } }, craftStack: 2);
        AddBlockDefinition(ID.BackroomBlock, 200, 2, SfxID.HitStone, materials: new Dictionary<ID, int> { { ID.DirtBlock, 1 } }, craftStack: 2);
        AddBlockDefinition(ID.StoneBlock, 150, 1, SfxID.HitStone);
        loot = Loot.CreateTable(ID.StoneBlock);
        loot.Add(1, 3, ID.Gravel);
        loot.Add(0.5f, 1, ID.MetalChunks);
        loot.Add(0.5f, 1, ID.MetalChunks);
        AddBlockDefinition(ID.WoodBlock, 100, 2, SfxID.HitStone);
        AddBlockDefinition(ID.GraniteBlock, 100, 2, SfxID.HitStone);

        // Materials
        AddMaterialDefinition(ID.Bullet, materials: new Dictionary<ID, int> { { ID.Charcoal, 1 }, { ID.Gravel, 2 }, { ID.Casing, 1 }}, craftStack: 5, time:1500);
        AddMaterialDefinition(ID.Casing);
        AddMaterialDefinition(ID.Sulphur);
        AddMaterialDefinition(ID.Chicken);
        AddMaterialDefinition(ID.Meat);
        AddMaterialDefinition(ID.CookedMeat, materials: new Dictionary<ID, int> { { ID.Meat, 1 } }, time:2000);
        AddMaterialDefinition(ID.Gravel);
        AddMaterialDefinition(ID.Sticks);
        AddMaterialDefinition(ID.Cytoplasm);
        AddMaterialDefinition(ID.Acorn);
        AddMaterialDefinition(ID.Paper);
        AddMaterialDefinition(ID.Wool);
        AddMaterialDefinition(ID.Fabric, materials: new Dictionary<ID, int> { { ID.Wool, 2 } }, time:1500);
        AddMaterialDefinition(ID.Flint);
        AddMaterialDefinition(ID.MetalChunks);
        AddMaterialDefinition(ID.Charcoal, materials: new Dictionary<ID, int> { { ID.Log, 2 } }, time:1500);
        AddMaterialDefinition(ID.Steel, materials: new Dictionary<ID, int> { { ID.MetalChunks, 3 }, {ID.Charcoal, 2} }, time:2000);
        AddMaterialDefinition(ID.Brick, materials: new Dictionary<ID, int> { { ID.Slag, 3 } }, time:1500);
        AddMaterialDefinition(ID.Stake, materials: new Dictionary<ID, int> { { ID.Log, 3 } });
        AddMaterialDefinition(ID.Slag, materials: new Dictionary<ID, int> { { ID.Gravel, 3 }, {ID.Charcoal, 2} }, time:1500);
        AddMaterialDefinition(ID.Log);
        AddMaterialDefinition(ID.Plank, materials: new Dictionary<ID, int> { { ID.Log, 3 } }, time:1500);

        // Structures
        AddStructureDefinition(ID.Chest, new Dictionary<ID, int> { { ID.Plank, 5 } }, 100);
        AddStructureDefinition(ID.Station, new Dictionary<ID, int> { { ID.Log, 15 }, { ID.Flint, 5 } }, 100);
        AddStructureDefinition(ID.Workbench, new Dictionary<ID, int> { { ID.Log, 15 } }, 100);
        AddStructureDefinition(ID.Stonecutter, new Dictionary<ID, int> { { ID.Steel, 2 }, { ID.Slag, 6 }, { ID.Plank, 5 }}, 100);
        AddStructureDefinition(ID.Sawmill, new Dictionary<ID, int> { { ID.Steel, 1 }, { ID.Slag, 6 }, { ID.Log, 5 }}, 100);
        AddStructureDefinition(ID.Campfire, new Dictionary<ID, int> { { ID.Gravel, 4 }, { ID.Flint, 1 }, { ID.Log, 15 }}, 100);
        AddStructureDefinition(ID.Furnace, new Dictionary<ID, int> { { ID.Log, 5 }, { ID.Gravel, 15 } }, 100);
        AddStructureDefinition(ID.Anvil, new Dictionary<ID, int> { { ID.Steel, 8 }, { ID.Hammer, 1}}, 100);
        AddStructureDefinition(ID.BlueprintStation, new Dictionary<ID, int> { { ID.Stake, 10 }, { ID.Flint, 2 } }, 100);

        // Tools
        AddToolDefinition(
            id: ID.SteelSword,
            gesture: ItemGesture.Swing,
            speed: 1.3f,
            range: 1,
            projectileInfo: new SwingProjectileInfo {
                Damage = 2,
                Knockback = 15,
                CritChance = 10,
                Speed = 1.3f,
                Radius = 2f,
                Breaking = 1,
                OperationType = OperationType.Cutting
            },
            materials: new Dictionary<ID, int> { { ID.Flint, 5 }, { ID.Stake, 2 } },
            holdoutOffset: new Vector2(0.6f, 0)
        );

        AddToolDefinition(
            id: ID.Blueprint,
            gesture: ItemGesture.Swing,
            sfx: SfxID.Null,
            speed: 4f,
            range: 7f,
            durability: -1,
            materials: new Dictionary<ID, int> { { ID.Log, 2 } },
            holdoutOffset: new Vector2(0.65f, 0)
        );

        AddToolDefinition(
            id: ID.StonePickaxe,
            gesture: ItemGesture.Swing,
            speed: 1.4f,
            range: 4f,
            projectileInfo: new SwingProjectileInfo {
                Damage = 1,
                Knockback = 10,
                CritChance = 10,
                Speed = 4f,
                Radius = 2,
                Breaking = 1,
                OperationType = OperationType.Mining
            },
            materials: new Dictionary<ID, int> { { ID.Sticks, 2 }, { ID.Flint, 4 } },
            holdoutOffset: new Vector2(0.65f, 0)
        );

        AddToolDefinition(
            id: ID.StoneHatchet,
            gesture: ItemGesture.Swing,
            speed: 1.4f,
            range: 4f,
            projectileInfo: new SwingProjectileInfo {
                Damage = 1,
                Knockback = 10,
                CritChance = 10,
                Speed = 2,
                Radius = 2,
                Breaking = 1,
                OperationType = OperationType.Cutting
            },
            materials: new Dictionary<ID, int> { { ID.Sticks, 2 }, { ID.Flint, 3 } },
            holdoutOffset: new Vector2(0.65f, 0)
        );

        AddToolDefinition(
            id: ID.MetalAxe,
            gesture: ItemGesture.Swing,
            speed: 1.4f,
            range: 4f,
            projectileInfo: new SwingProjectileInfo {
                Damage = 1,
                Knockback = 10,
                CritChance = 10,
                Speed = 2,
                Radius = 2,
                Breaking = 2,
                OperationType = OperationType.Cutting
            },
            materials: new Dictionary<ID, int> { { ID.Steel, 2 }, { ID.Stake, 5 } },
            holdoutOffset: new Vector2(0.65f, 0)
        );

        AddToolDefinition(
            id: ID.DiamondAxe,
            gesture: ItemGesture.Swing,
            speed: 1.4f,
            range: 4f,
            projectileInfo: new SwingProjectileInfo {
                Damage = 1,
                Knockback = 10,
                CritChance = 10,
                Speed = 2,
                Radius = 2,
                Breaking = 4,
                OperationType = OperationType.Cutting
            },
            materials: new Dictionary<ID, int> { { ID.Brick, 1 }, { ID.Stake, 2 } },
            holdoutOffset: new Vector2(0.65f, 0)
        );

        AddToolDefinition(
            id: ID.Hammer,
            gesture: ItemGesture.Swing,
            speed: 1.8f,
            range: 4f,
            projectileInfo: new SwingProjectileInfo {
                Damage = 1,
                Knockback = 10,
                CritChance = 10,
                Speed = 2,
                Radius = 2,
                Breaking = 3,
                OperationType = OperationType.Building
            },
            materials: new Dictionary<ID, int> { { ID.Flint, 2 }, { ID.Sticks, 2 } },
            holdoutOffset: new Vector2(0.65f, 0)
        );

        AddToolDefinition(
            id: ID.Spear,
            gesture: ItemGesture.Swing,
            speed: 0.8f,
            projectileInfo: new RangedProjectileInfo {
                Sprite = ID.Spear,
                Damage = 2,
                Knockback = 10,
                CritChance = 10,
                LifeSpan = 10000,
                Speed = 60,
                Radius = 0.3f,
                Penetration = 1,
                Lodge = true,
                PickUp = true,
                Ammo = ID.Spear
            },
            materials: new Dictionary<ID, int> { { ID.Flint, 2 }, { ID.Stake, 2 } },
            projectileOffset: 1.016f,
            stackSize: 20,
            holdoutOffset: new Vector2(0.65f, 0),
            rotationOffset: 45
        );
        AddToolDefinition(
            id: ID.Minigun,
            gesture: ItemGesture.Shoot,
            speed: 2f,
            sfx: SfxID.Minigun,
            projectileInfo: new RangedProjectileInfo {
                Sprite = ID.BulletProjectile,
                Damage = 1000,
                Knockback = 5,
                CritChance = 10,
                LifeSpan = 10000,
                Speed = 60,
                Radius = 0.1f,
                Penetration = 1, 
            },
            materials: new Dictionary<ID, int> { { ID.WoodBlock, 2 } },
            projectileOffset: 1.4f,
            durability: -1,
            holdoutOffset: new Vector2(0.4f, 0)
        );

        AddToolDefinition(
            id: ID.Pistol,
            gesture: ItemGesture.Shoot,
            speed: 0.6f,
            sfx: SfxID.Pistol,
            projectileInfo: new RangedProjectileInfo {
                Sprite = ID.BulletProjectile,
                Damage = 1,
                Knockback = 6,
                CritChance = 10,
                LifeSpan = 10000,
                Speed = 60,
                Radius = 0.1f,
                Ammo = ID.Bullet,
                Penetration = 1, 
            },
            materials: new Dictionary<ID, int> { { ID.WoodBlock, 2 } },
            projectileOffset: 1.016f,
            holdoutOffset: new Vector2(0.4f, -0.25f)
        );
    }
}