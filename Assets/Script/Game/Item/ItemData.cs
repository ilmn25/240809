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
        AddBlockDefinition(ID.BrickBlock, 150, 1, SfxID.HitMetal, description: "A heavy brick block used for durable structures.", materials: new Dictionary<ID, int> { { ID.Gravel, 3 } });
        loot = new (ID.BrickBlock);
        loot.Add(1, 3, ID.Gravel);
        loot.Add(0.5f, 1, ID.Slag);
        loot.Add(0.5f, 1, ID.Brick);
        AddBlockDefinition(ID.MarbleBlock, 200, 1, SfxID.HitMetal, description: "Polished marble block with a pristine look.", materials: new Dictionary<ID, int> { { ID.StoneBlock, 1 }, { ID.BrickBlock, 1 } }, craftStack: 2);
        AddBlockDefinition(ID.DirtBlock, 80, 1, SfxID.HitStone, description: "Basic dirt block, useful for filling and farming.");
        loot = new (ID.DirtBlock);
        loot.Add(1, 3, ID.Mud);
        loot.Add(0.5f, 1, ID.Flint);
        loot.Add(0.5f, 1, ID.Sticks);
        AddBlockDefinition(ID.SandBlock, 40, 1, SfxID.HitSand, description: "Fine sand block used for glassmaking and terrain.", materials: new Dictionary<ID, int> { { ID.StoneBlock, 1 } }, craftStack: 2);
        AddBlockDefinition(ID.BackroomBlock, 200, 2, SfxID.HitStone, description: "Mysterious block from the backroom; fragile to danger.", materials: new Dictionary<ID, int> { { ID.DirtBlock, 1 } }, craftStack: 2);
        AddBlockDefinition(ID.StoneBlock, 150, 1, SfxID.HitStone, description: "Common stone block, the foundation of many builds.");
        loot = new (ID.StoneBlock);
        loot.Add(1, 3, ID.Gravel);
        loot.Add(0.5f, 1, ID.MetalChunks);
        loot.Add(0.5f, 1, ID.MetalChunks);
        loot.Add(0.5f, 1, ID.CopperChunks);
        loot.Add(0.5f, 1, ID.CopperChunks);
        AddBlockDefinition(ID.WoodBlock, 100, 2, SfxID.HitStone, description: "Wood block from trees; a basic construction material.");
        AddBlockDefinition(ID.GraniteBlock, 100, 2, SfxID.HitStone, description: "Hard granite block, tough and reliable.");
        AddBlockDefinition(
            ID.Chalk,
            description: "Blueprint used to learn new crafting recipes.",
            materials: new Dictionary<ID, int> { { ID.Log, 2 }, { ID.Charcoal, 1 } }
        );

        // Materials
        AddMaterialDefinition(ID.Bullet, "High-explosive bullet rounds for guns.", materials: new Dictionary<ID, int> { { ID.Charcoal, 1 }, { ID.Gravel, 2 }, { ID.Casing, 1 }}, craftStack: 5, time:1500);
        AddMaterialDefinition(ID.Casing, "Empty shell casing for ammunition.");
        AddMaterialDefinition(ID.Sulphur, "Powdered explosive component.");
        AddMaterialDefinition(ID.Foul, "Fresh poultry meat from wild fowl.");
        AddMaterialDefinition(ID.Meat, "Raw meat, can be cooked for better healing.");
        AddMaterialDefinition(ID.CookedMeat, "Cooked meat, restores more health than raw.", materials: new Dictionary<ID, int> { { ID.Meat, 1 } }, time:2000);
        AddMaterialDefinition(ID.CookedChicken, "Cooked chicken, restores more health than raw.", materials: new Dictionary<ID, int> { { ID.Foul, 1 } }, time:2000);
        AddMaterialDefinition(ID.Sand, "Loose sand collected from desert debris and deposits.");
        AddMaterialDefinition(ID.Glass, "Smelted glass used for windows and light fixtures.", materials: new Dictionary<ID, int> { { ID.Sand, 2 } }, time: 1800);
        AddMaterialDefinition(ID.Shell, "A tiny collection of shells found on the ground.");
        AddMaterialDefinition(ID.Mud, "Wet earth used for mulch and ground work.");
        AddMaterialDefinition(ID.Gravel, "Loose gravel chunks for crafting and building.");
        AddMaterialDefinition(ID.Sticks, "A small bundle of sticks for tools and torches.");
        AddMaterialDefinition(ID.Cytoplasm, "Strange viscous substance from creatures.");
        AddMaterialDefinition(ID.Acorn, "A tiny seed, useful for planting or crafting.");
        AddMaterialDefinition(ID.Paper, "Thin paper used for notes or blueprints.");
        AddMaterialDefinition(ID.Wool, "Soft wool, used for cloth and insulation.");
        AddMaterialDefinition(ID.Fabric, "Woven fabric, ideal for wearable gear.", materials: new Dictionary<ID, int> { { ID.Wool, 2 } }, time:1500);
        AddMaterialDefinition(ID.Flint, "Sharp stone shards for toolmaking.");
        AddMaterialDefinition(ID.MetalChunks, "Chunks of metal for smelting.");
        AddMaterialDefinition(ID.CopperChunks, "Chunks of copper ore for smelting.");
        AddMaterialDefinition(ID.Copper, "Strong forged copper for crafting and building.", materials: new Dictionary<ID, int> { { ID.CopperChunks, 3 }, { ID.Charcoal, 2 } }, time:2000);
        AddMaterialDefinition(ID.Charcoal, "Burned wood fuel for smelting.", materials: new Dictionary<ID, int> { { ID.Log, 2 } }, time:1500);
        AddMaterialDefinition(ID.Steel, "Strong forged metal for high-tier items.", materials: new Dictionary<ID, int> { { ID.MetalChunks, 3 }, {ID.Charcoal, 2} }, time:2000);
        AddMaterialDefinition(ID.Brick, "Refined brick piece for sturdy construction.", materials: new Dictionary<ID, int> { { ID.Slag, 3 } }, time:1500);
        AddMaterialDefinition(ID.Stake, "Wooden stake for defense and traps.", materials: new Dictionary<ID, int> { { ID.Log, 3 } });
        AddMaterialDefinition(ID.Slag, "Molten industrial residue used in crafting.", materials: new Dictionary<ID, int> { { ID.Gravel, 3 }, {ID.Charcoal, 2} }, time:1500);
        AddMaterialDefinition(ID.Log, "A solid tree log, base for many constructions.");
        AddMaterialDefinition(ID.Plank, "Wooden plank crafted from logs.", materials: new Dictionary<ID, int> { { ID.Log, 3 } }, time:1500);

        // Structures
        AddStructureDefinition(ID.Chest, new Dictionary<ID, int> { { ID.Plank, 5 } }, 100, description: "A storage chest for keeping loot safe.");
        AddStructureDefinition(ID.Workbench, new Dictionary<ID, int> { { ID.Log, 15 }, { ID.Flint, 5 } }, 100, description: "A workbench to build other machines.");
        AddStructureDefinition(ID.Smelter, new Dictionary<ID, int> { { ID.Log, 8 }, { ID.StoneBlock, 6 } }, 100, description: "A dedicated smelter used to turn sand into glass.");
        AddStructureDefinition(ID.MasonryWorkbench, new Dictionary<ID, int> { { ID.Log, 10 }, { ID.StoneBlock, 8 } }, 100, description: "A masonry bench for building advanced stonework and powering anvils.");
        AddStructureDefinition(ID.WoodenToolbench, new Dictionary<ID, int> { { ID.Log, 15 } }, 100, description: "Crafts basic tools like spear, pickaxe and hammer.");
        AddStructureDefinition(ID.CarpenterWorkbench, new Dictionary<ID, int> { { ID.Log, 10 }, { ID.Plank, 5 } }, 100, description: "Crafts beds, looms and signs.");
        AddStructureDefinition(ID.Loom, new Dictionary<ID, int> { { ID.Plank, 6 }, { ID.Sticks, 2 } }, 100, description: "Weaves wool into fabric.");
        AddStructureDefinition(ID.Bed, new Dictionary<ID, int> { { ID.Fabric, 3 }, { ID.Plank, 3 } }, 100, description: "A place to sleep through the night.");
        AddStructureDefinition(ID.Sign, new Dictionary<ID, int> { { ID.Plank, 2 }, { ID.Sticks, 2 } }, 100, description: "A wooden sign for labels and notes.");
        AddBlockDefinition(ID.MulchBlock, 80, 1, SfxID.HitStone, description: "A soft ground block made from mud and sticks.", materials: new Dictionary<ID, int> { { ID.Mud, 2 }, { ID.Sticks, 2 } });
        AddStructureDefinition(ID.Stonecutter, new Dictionary<ID, int> { { ID.Steel, 2 }, { ID.Slag, 6 }, { ID.Plank, 5 }}, 100, description: "Cuts stone into refined brick parts quickly.");
        AddStructureDefinition(ID.Sawmill, new Dictionary<ID, int> { { ID.Steel, 1 }, { ID.Slag, 6 }, { ID.Log, 5 }}, 100, description: "Transforms logs into planks and stakes for building.");
        AddStructureDefinition(ID.Campfire, new Dictionary<ID, int> { { ID.Gravel, 4 }, { ID.Flint, 1 }, { ID.Log, 15 }}, 100, description: "Cooks meat and creates charcoal while giving light.");
        AddStructureDefinition(ID.Furnace, new Dictionary<ID, int> { { ID.Log, 5 }, { ID.Gravel, 15 } }, 100, description: "Smelts metal materials into slag and steel.");
        AddStructureDefinition(ID.Anvil, new Dictionary<ID, int> { { ID.Steel, 8 }, { ID.Hammer, 1}}, 100, description: "Forges steel weapons and tools at higher quality.");
        AddStructureDefinition(ID.BlueprintStation, new Dictionary<ID, int> { { ID.Stake, 10 }, { ID.Flint, 2 } }, 100, description: "Unlocks blueprint-based crafting options.");
        AddStructureDefinition(ID.FieldStation, new Dictionary<ID, int> { { ID.Log, 8 }, { ID.Sticks, 4 } }, 100, description: "A lightweight station specialized for field planting tools.");
        AddStructureDefinition(ID.ImprovisedPlanter, new Dictionary<ID, int> { { ID.Log, 3 }, { ID.Sticks, 2 } }, 100, description: "A basic planter. Feed it an acorn to grow a log.");

        // Tools
        AddToolDefinition(
            id: ID.SteelSword,
            gesture: ItemGesture.Swing,
            speed: 1.3f,
            range: 1,
            projectileInfo: new SwingProjectileInfo {
                Damage = 2,
                Knockback = 11,
                CritChance = 10,
                Speed = 1f,
                Radius = 1.5f,
                Breaking = 1,
                OperationType = OperationType.Cutting
            },
            description: "A sharp steel sword, balanced for battle.",
            materials: new Dictionary<ID, int> { { ID.Flint, 5 }, { ID.Stake, 2 } },
            holdoutOffset: new Vector2(0.6f, 0)
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
            description: "Basic pickaxe for gathering stone and ores.",
            materials: new Dictionary<ID, int> { { ID.Sticks, 2 }, { ID.Flint, 4 } },
            holdoutOffset: new Vector2(0.65f, 0)
        );

        AddToolDefinition(
            id: ID.CrudePickaxe,
            gesture: ItemGesture.Swing,
            speed: 2.4f,
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
            durability: 100,
            description: "A rough pickaxe for early mining.",
            materials: new Dictionary<ID, int> { { ID.Sticks, 2 }, { ID.Flint, 3 } },
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
            description: "Stone hatchet for chopping wood.",
            materials: new Dictionary<ID, int> { { ID.Sticks, 2 }, { ID.Flint, 3 } },
            holdoutOffset: new Vector2(0.65f, 0)
        );

        AddToolDefinition(
            id: ID.CrudeHatchet,
            gesture: ItemGesture.Swing,
            speed: 2.4f,
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
            durability: 100,
            description: "A rough hatchet for early woodcutting.",
            materials: new Dictionary<ID, int> { { ID.Sticks, 2 }, { ID.Flint, 2 } },
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
            description: "Axe forged from metal for tougher timber.",
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
            description: "A premium diamond axe for efficient cutting.",
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
            description: "Hammer used for building and weapon crafting.",
            materials: new Dictionary<ID, int> { { ID.Flint, 2 }, { ID.Sticks, 2 } },
            holdoutOffset: new Vector2(0.65f, 0)
        );

        AddToolDefinition(
            id: ID.CrudeMallet,
            gesture: ItemGesture.Swing,
            speed: 2.8f,
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
            durability: 100,
            description: "A rough mallet for basic building work.",
            materials: new Dictionary<ID, int> { { ID.Flint, 1 }, { ID.Sticks, 2 } },
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
            description: "A throwable spear that can pierce enemies.",
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
                Damage = 3,
                Knockback = 5,
                CritChance = 10,
                LifeSpan = 10000,
                Speed = 60,
                Radius = 0.1f,
                Ammo = ID.Bullet,
                Penetration = 1, 
            },
            description: "Squad-fire minigun, unleashes a hail of bullets.",
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
                Damage = 5,
                Knockback = 6,
                CritChance = 10,
                LifeSpan = 10000,
                Speed = 60,
                Radius = 0.1f,
                Ammo = ID.Bullet,
                Penetration = 1, 
            },
            description: "Compact pistol, quick draw and moderate damage.",
            materials: new Dictionary<ID, int> { { ID.WoodBlock, 2 } },
            projectileOffset: 1.016f,
            holdoutOffset: new Vector2(0.4f, -0.25f)
        );
    }
}