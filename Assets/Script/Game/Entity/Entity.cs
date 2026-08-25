using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
public class Entity
{ 
        public static readonly Dictionary<ID, Entity> Dictionary = new Dictionary<ID, Entity>();
        
        public Vector3 Bounds;
        public ID PrefabName;
        public Type Machine;
        public int Collision;
        public bool StaticLoad;
        public Vector3 SpawnOffset;
        /// <summary>NavMap value this static entity writes when placed (Block for most structures, Door for doors).</summary>
        public byte NavValue = NavMap.Block;
        
        public static Vector3 MidAir = new Vector3(0.5f, 0.3f, 0.5f);
        public static Vector3 Floor = new Vector3(0.5f, 0f, 0.5f);
          
        
        private static readonly Entity Block = new Entity
        {
                Bounds = Vector3Int.one,
                Collision = Main.IndexSemiCollide,
                PrefabName = ID.BlockPrefab,
                Machine = typeof(BlockMachine),
                StaticLoad = true,
                SpawnOffset = Floor,
        }; 
        // The mining box is a non-solid overlay on an existing block — it owns no
        // collision or nav of its own, so the block underneath keeps its nav intact.
        private static readonly Entity MiningBoxEntity = new Entity
        {
                Bounds = Vector3Int.one,
                Collision = Main.IndexNoCollide,
                PrefabName = ID.BlockPrefab,
                Machine = typeof(BlockMachine),
                StaticLoad = true,
                SpawnOffset = Floor,
        }; 
        public static void Initialize()
        {
                Loot loot;

                AddStructure<PineTreeMachine>(ID.PineTree, new Vector3Int(1, 3, 1), Main.IndexCollide);
                loot = new (ID.PineTree);
                loot.Add(1, 4, ID.Log);
                loot.Add(0.5f, 1, ID.Log);
                loot.Add(0.5f, 1, ID.Acorn);
                loot.Add(1, 1, ID.Sticks);
                loot.Add(0.7f, 1, ID.Sticks);
                loot.Add(0.5f, 1, ID.Sticks);

                AddStructure<BirchTreeMachine>(ID.BirchTree, new Vector3Int(1, 3, 1), Main.IndexCollide);
                loot = new (ID.BirchTree);
                loot.Add(1, 6, ID.Log);
                loot.Add(0.8f, 2, ID.Log);
                loot.Add(0.6f, 1, ID.Acorn);
                loot.Add(1, 1, ID.Sticks);
                loot.Add(0.7f, 1, ID.Sticks);
                loot.Add(0.5f, 1, ID.Sticks);
                
                AddStructure<OakTreeMachine>(ID.OakTree, new Vector3Int(1, 3, 1), Main.IndexCollide);
                loot = new (ID.OakTree);
                loot.Add(1, 8, ID.Log);
                loot.Add(0.8f, 3, ID.Log);
                loot.Add(0.6f, 1, ID.Acorn);
                loot.Add(1, 1, ID.Sticks);
                loot.Add(0.7f, 2, ID.Sticks);
                
                AddStructure<StoneBoulderMachine>(ID.StoneBoulder, Vector3Int.one, Main.IndexSemiCollide, NavMap.Semi);
                loot = new (ID.StoneBoulder);
                loot.Add(1, 2, ID.Gravel);
                loot.Add(0.5f, 1, ID.Gravel);
                loot.Add(1, 1, ID.Flint);
                loot.Add(0.7f, 1, ID.Flint);
                loot.Add(0.5f, 1, ID.Flint);
                loot.Add(0.15f, 1, ID.Geode);

                AddStructure<IronDepositMachine>(ID.IronDeposit, Vector3Int.one, Main.IndexSemiCollide, NavMap.Semi);
                loot = new (ID.IronDeposit);
                loot.Add(1, 2, ID.MetalChunks);
                loot.Add(0.6f, 2, ID.MetalChunks);
                loot.Add(0.5f, 1, ID.Gravel);
                loot.Add(0.5f, 1, ID.Flint);

                AddStructure<MeteorMachine>(ID.Meteor, Vector3Int.one, Main.IndexSemiCollide, NavMap.Semi);
                loot = new (ID.Meteor);
                loot.Add(1, 3, ID.Meteorite);
                loot.Add(0.7f, 2, ID.Meteorite);
                loot.Add(0.5f, 1, ID.MetalChunks);
                loot.Add(0.5f, 1, ID.CopperChunks);

                AddStructure<SandSlabMachine>(ID.SandSlab, Vector3Int.one, Main.IndexSemiCollide, NavMap.Semi);
                loot = new (ID.SandSlab);
                loot.Add(1, 2, ID.Sand);
                loot.Add(0.7f, 1, ID.Sand);
                loot.Add(0.5f, 1, ID.Gravel);
                loot.Add(0.5f, 1, ID.Flint);
                loot.Add(0.1f, 1, ID.Geode);

                AddStructure<SandDebrisMachine>(ID.SandDebris, Vector3Int.one, Main.IndexSemiCollide, NavMap.Semi);
                loot = new (ID.SandDebris);
                loot.Add(1, 2, ID.Sand);
                loot.Add(0.7f, 1, ID.Sand);
                loot.Add(0.5f, 1, ID.Mud);
                loot.Add(0.3f, 1, ID.Flint);
                loot.Add(0.1f, 1, ID.Geode);
                
                AddStructure<ComputerMachine>(ID.Computer, Vector3Int.one, Main.IndexCollide);
                AddStructure<ToolbenchMachine>(ID.Toolbench, Vector3Int.one, Main.IndexCollide);
                AddStructure<CarpenterWorkbenchMachine>(ID.CarpenterWorkbench, Vector3Int.one, Main.IndexCollide);
                AddStructure<LoomMachine>(ID.Loom, Vector3Int.one, Main.IndexCollide);
                AddStructure<FurnaceMachine>(ID.Furnace, Vector3Int.one, Main.IndexCollide);
                AddStructure<SmelterMachine>(ID.Smelter, Vector3Int.one, Main.IndexCollide);
                AddStructure<StonecutterMachine>(ID.Stonecutter, Vector3Int.one, Main.IndexCollide);
                AddStructure<CampfireMachine>(ID.Campfire, Vector3Int.one, Main.IndexCollide);
                AddStructure<SawmillMachine>(ID.Sawmill, Vector3Int.one, Main.IndexCollide);
                AddStructure<MasonryWorkbenchMachine>(ID.MasonryWorkbench, Vector3Int.one, Main.IndexCollide);
                AddStructure<AnvilMachine>(ID.Anvil, Vector3Int.one, Main.IndexCollide);
                AddStructure<FieldStationMachine>(ID.FieldStation, Vector3Int.one, Main.IndexCollide);
                AddStructure<PulverizerMachine>(ID.Pulverizer, Vector3Int.one, Main.IndexCollide);
                AddStructure<RefineryMachine>(ID.Refinery, Vector3Int.one, Main.IndexCollide);
                AddStructure<ImprovisedPlanterMachine>(ID.ImprovisedPlanter, Vector3Int.one, Main.IndexSemiCollide, NavMap.Semi);
                AddStructure<CrockPotMachine>(ID.CrockPot, Vector3Int.one, Main.IndexSemiCollide, NavMap.Semi);
                AddStructure<PondMachine>(ID.Pond, Vector3Int.one, Main.IndexSemiCollide, NavMap.Semi);
                AddStructure<SprinklerMachine>(ID.Sprinkler, Vector3Int.one, Main.IndexSemiCollide, NavMap.Semi);
                AddStructure<LightningRodMachine>(ID.LightningRod, Vector3Int.one, Main.IndexSemiCollide, NavMap.Semi);
                AddStructure<WorkbenchMachine>(ID.Workbench, Vector3Int.one, Main.IndexCollide);
                AddStructure<DoorMachine>(ID.Door, new Vector3Int(1, 2, 1), Main.IndexCollide, NavMap.Door);
                AddStructure<DungeonDoorMachine>(ID.DungeonDoor, new Vector3Int(1, 2, 1), Main.IndexCollide, NavMap.Door);

                loot = new (ID.Workbench);
                loot.Add(1, 3, ID.Log);
                loot.Add(0.5f, 4, ID.Log);
                loot.Add(1, 1, ID.Flint);
                loot.Add(0.5f, 1, ID.Flint);

                loot = new (ID.Toolbench);
                loot.Add(1, 3, ID.Log);
                loot.Add(0.5f, 4, ID.Log);

                loot = new (ID.CarpenterWorkbench);
                loot.Add(1, 3, ID.Log);
                loot.Add(0.5f, 2, ID.Plank);

                loot = new (ID.Loom);
                loot.Add(1, 2, ID.Plank);
                loot.Add(0.5f, 2, ID.Sticks);
                loot.Add(0.5f, 1, ID.Wool);

                loot = new (ID.Stonecutter);
                loot.Add(0.5f, 1, ID.Steel);
                loot.Add(0.5f, 1, ID.Steel);
                loot.Add(1, 1, ID.Slag);
                loot.Add(0.5f, 2, ID.Slag);
                loot.Add(1, 1, ID.Plank);
                loot.Add(0.5f, 1, ID.Plank);

                loot = new (ID.Sawmill);
                loot.Add(1, 1, ID.Slag);
                loot.Add(0.5f, 2, ID.Slag);
                loot.Add(1, 1, ID.Log);
                loot.Add(0.5f, 1, ID.Log);

                loot = new (ID.Campfire);
                loot.Add(1, 1, ID.Gravel);
                loot.Add(0.5f, 1, ID.Gravel);
                loot.Add(1, 3, ID.Log);
                loot.Add(0.5f, 4, ID.Log);

                loot = new (ID.Furnace);
                loot.Add(1, 1, ID.Log);
                loot.Add(0.5f, 1, ID.Log);
                loot.Add(1, 3, ID.Gravel);
                loot.Add(0.5f, 4, ID.Gravel);

                loot = new (ID.Anvil);
                loot.Add(1, 2, ID.Steel);
                loot.Add(0.5f, 2, ID.Steel);

                loot = new (ID.ImprovisedPlanter);
                loot.Add(1, 2, ID.Log);
                loot.Add(0.5f, 1, ID.Acorn);
                loot.Add(0.25f, 1, ID.CornSeed);
                loot.Add(0.25f, 1, ID.PumpkinSeed);

                loot = new (ID.CrockPot);
                loot.Add(1, 2, ID.Steel);
                loot.Add(1, 3, ID.StoneBlock);

                loot = new (ID.FieldStation);
                loot.Add(1, 2, ID.Log);
                loot.Add(0.5f, 2, ID.Sticks);

                loot = new (ID.Pulverizer);
                loot.Add(1, 2, ID.Steel);
                loot.Add(0.5f, 2, ID.Steel);
                loot.Add(1, 2, ID.StoneBlock);

                loot = new (ID.Refinery);
                loot.Add(1, 3, ID.Steel);
                loot.Add(0.5f, 2, ID.Glass);
                loot.Add(1, 4, ID.StoneBlock);
                
                AddStructure<BasicChestMachine>(ID.Chest, Vector3Int.one, Main.IndexCollide);
                loot = new (ID.Chest);
                loot.Add(1, 1, ID.MetalChunks);
                loot.Add(0.6f, 2, ID.Steel);
                loot.Add(0.6f, 2, ID.Copper);
                loot.Add(1, 3, ID.Brick); 
                loot.Add(0.7f, 1, ID.Charcoal, ID.Flint);
                loot.Add(1, 1, ID.Spear, ID.StoneHatchet); 
                loot.Add(0.35f, 1, ID.OldRadio); 

                AddStructure<SkeletonMachine>(ID.Skeleton, Vector3Int.one, Main.IndexSemiCollide, NavMap.Semi);
                
                AddStructure<HarvestableMachine>(ID.Bush, Vector3Int.one, Main.IndexNoCollide);
                // Harvestable plants need a non-zero bounds so a collider is added
                // for raycast interaction (they stay on NoCollide so they don't block movement).
                AddStructure<HarvestableMachine>(ID.Grass, Vector3Int.one, Main.IndexNoCollide);
                AddStructure<HarvestableMachine>(ID.Deathcap, Vector3Int.one, Main.IndexNoCollide);
                AddStructure<HarvestableMachine>(ID.Orchids, Vector3Int.one, Main.IndexNoCollide);   
                AddStructure<HarvestableMachine>(ID.Tulip, Vector3Int.one, Main.IndexNoCollide);
                AddStructure<HarvestableMachine>(ID.Daisies, Vector3Int.one, Main.IndexNoCollide);
                AddStructure<BedMachine>(ID.Bed, Vector3Int.one, Main.IndexSemiCollide, NavMap.Semi);
                AddStructure<SignMachine>(ID.Sign, Vector3Int.one, Main.IndexSemiCollide, NavMap.Semi);
                loot = new (ID.Bed);
                loot.Add(1, 3, ID.Fabric);
                loot.Add(1, 3, ID.Plank);
                loot = new (ID.Sign);
                loot.Add(1, 2, ID.Plank);
                loot.Add(1, 2, ID.Sticks);
                AddStructure<PortalMachine>(ID.Portal, Vector3Int.one, Main.IndexSemiCollide, NavMap.Semi);
                AddStructure<HarvestableMachine>(ID.Table, Vector3Int.one, Main.IndexCollide);
                // Burn results — spawned when flammable objects burn out.
                AddStructure<BurnedTreeMachine>(ID.BurnedTree, new Vector3Int(1, 2, 1), Main.IndexCollide);
                AddStructure<RubbleMachine>(ID.Rubble, Vector3Int.one, Main.IndexSemiCollide, NavMap.Semi);
                AddStructure<CharredRubbleMachine>(ID.CharredRubble, Vector3Int.one, Main.IndexSemiCollide, NavMap.Semi);
                loot = new (ID.Rubble);
                loot.Add(1, 1, ID.Gravel);
                loot.Add(0.5f, 1, ID.Gravel);
                loot = new (ID.CharredRubble);
                loot.Add(1, 1, ID.Gravel);
                loot.Add(0.5f, 1, ID.Gravel);
                AddStructure<LampMachine>(ID.Lamp, Vector3Int.one, Main.IndexCollide);
                AddStructure<GeneratorMachine>(ID.Generator, Vector3Int.one, Main.IndexCollide);
                AddStructure<OwlStatueMachine>(ID.OwlStatue, Vector3Int.one, Main.IndexCollide);
                loot = new (ID.OwlStatue);
                loot.Add(1, 2, ID.StoneBlock);
                loot.Add(0.5f, 2, ID.StoneBlock);

                AddStructure<BulletinBoardMachine>(ID.BulletinBoard, Vector3Int.one, Main.IndexCollide);
                loot = new (ID.BulletinBoard);
                loot.Add(1, 2, ID.Plank);
                loot.Add(0.5f, 2, ID.Sticks);

                AddStructure<HeadstoneMachine>(ID.Headstone, Vector3Int.one, Main.IndexCollide);
                loot = new (ID.Headstone);
                loot.Add(1, 1, ID.StoneBlock);

                AddStructure<SpiderNestMachine>(ID.SpiderNest, Vector3Int.one, Main.IndexCollide);
                loot = new (ID.SpiderNest);
                loot.Add(1, 2, ID.Foul);

                AddStructure<OldPotMachine>(ID.OldPot, Vector3Int.one, Main.IndexCollide);

                AddStructure<BarrelMachine>(ID.OilBarrel, Vector3Int.one, Main.IndexCollide);
                loot = new (ID.OilBarrel);
                loot.Add(1, 1, ID.Steel);
                loot.Add(0.5f, 2, ID.Plank);
                loot.Add(0.4f, 1, ID.Slag);

                AddStructure<BarrelMachine>(ID.Barrel, Vector3Int.one, Main.IndexCollide);
                loot = new (ID.Barrel);
                loot.Add(1, 2, ID.Plank);
                loot.Add(0.5f, 1, ID.Steel);

                AddStructure<HiveMachine>(ID.Hive, Vector3Int.one, Main.IndexCollide);
                loot = new (ID.Hive);
                loot.Add(1, 2, ID.BucketOfHoney);

                AddStructure<DirtyTentMachine>(ID.DirtyTent, Vector3Int.one, Main.IndexCollide);
                loot = new (ID.DirtyTent);
                loot.Add(1, 2, ID.Fabric);
                loot.Add(0.5f, 1, ID.Fabric);

                AddStructure<SpiderWebMachine>(ID.SpiderWeb, Vector3Int.one, Main.IndexNoCollide);
                AddStructure<OldRadioMachine>(ID.OldRadio, Vector3Int.one, Main.IndexCollide);
                loot = new (ID.OldRadio);
                AddStructure<ScarecrowMachine>(ID.Scarecrow, Vector3Int.one, Main.IndexCollide);
                loot = new (ID.Scarecrow);
                loot.Add(1, 2, ID.Steel);
                loot.Add(0.5f, 1, ID.Steel);
                loot.Add(1, 2, ID.Slag);
                AddStructure<DriedWellMachine>(ID.DriedWell, Vector3Int.one, Main.IndexCollide);
                AddStructure<ChairMachine>(ID.Chair, Vector3Int.one, Main.IndexCollide);
                AddStructure<BookshelfMachine>(ID.Bookshelf, Vector3Int.one, Main.IndexCollide);
                loot.Add(0.5f, 1, ID.Glass);
                loot = new (ID.Door);
                loot.Add(1, 2, ID.Plank);
                loot.Add(0.5f, 1, ID.Plank);
                loot = new (ID.Lamp);
                loot.Add(1, 1, ID.Glass);
                loot.Add(0.5f, 2, ID.Glass);
                loot.Add(1, 2, ID.Plank);
                loot.Add(0.7f, 1, ID.Stake); 
                loot = new (ID.Generator);
                loot.Add(1, 1, ID.Copper);
                loot.Add(1, 1, ID.Glass);
                loot.Add(0.5f, 2, ID.Stake); 
                 
                AddMob<PlayerMachine>(ID.Player);

                AddMob<CorpseMachine>(ID.Corpse);

                AddMob<ScoutMachine>(ID.Chito); 
                loot = new (ID.Chito);
                loot.Add(0.7f, 5, ID.Bullet); 
                loot.Add(0.1f, 1, ID.Pistol); 
                
                AddMob<ScoutMachine>(ID.Yuuri);
                loot = new (ID.Yuuri);
                loot.Add(0.7f, 5, ID.Bullet);  
                loot.Add(0.1f, 1, ID.Pistol);  

                AddMob<ScoutGuardMachine>(ID.ScoutGuard);
                loot = new (ID.ScoutGuard);
                loot.Add(0.7f, 5, ID.Bullet);
                loot.Add(0.1f, 1, ID.Pistol);
                
                AddMob<SheepMachine>(ID.Sheep);
                loot = new (ID.Sheep);
                loot.Add(1, 1, ID.Meat);  
                loot.Add(0.5f, 1, ID.Meat);  
                loot.Add(0.5f, 1, ID.Wool); 
                loot.Add(0.5f, 1, ID.Wool); 
                loot.Add(0.5f, 1, ID.Wool); 

                AddMob<ChickMachine>(ID.Chick);
                loot = new (ID.Chick); // chicks drop nothing

                AddMob<HenMachine>(ID.Hen);
                loot = new (ID.Hen);
                loot.Add(1, 1, ID.Foul);
                loot.Add(0.5f, 1, ID.Foul);
                loot.Add(0.4f, 1, ID.Egg);

                AddMob<RoosterMachine>(ID.Rooster);
                loot = new (ID.Rooster);
                loot.Add(1, 1, ID.Foul);
                loot.Add(0.5f, 1, ID.Foul);

                AddMob<GuideMachine>(ID.Guide);
                loot = new (ID.Guide); // the guide drops nothing

                AddMob<BoundNPCMachine>(ID.BoundNPC);
                loot = new (ID.BoundNPC); // the bound NPC drops nothing

                AddMob<MerchantMachine>(ID.Merchant);
                loot = new (ID.Merchant); // the merchant drops nothing

                AddMob<CollectorMachine>(ID.Collector);
                loot = new (ID.Collector); // the collector drops nothing

                AddMob<QuestmasterMachine>(ID.Questmaster);
                loot = new (ID.Questmaster); // the questmaster drops nothing

                AddMob<NomadMachine>(ID.Nomad);
                loot = new (ID.Nomad); // the nomads drop nothing

                AddMob<CaravanMachine>(ID.Caravan);
                loot = new (ID.Caravan); // the caravan drops nothing
                
                AddMob<BugMachine>(ID.SnareFlea); 
                loot = new (ID.SnareFlea);
                loot.Add(0.5f, 6, ID.Sticks); 
                
                AddMob<RaiderMachine>(ID.Raider);
                loot = new (ID.Raider);
                loot.Add(0.1f, 1, ID.SteelSword, ID.DiamondAxe); 

                AddMob<RaiderGuardMachine>(ID.RaiderGuard);
                loot = new (ID.RaiderGuard);
                loot.Add(0.1f, 1, ID.SteelSword, ID.DiamondAxe); 
                
                AddMob<SlimeMachine>(ID.Slime);
                loot =  new (ID.Slime);
                loot.Add(1, 2, ID.Cytoplasm); 

                AddMob<BabySlimeMachine>(ID.BabySlime);
                loot = new (ID.BabySlime);
                loot.Add(1, 1, ID.Cytoplasm);
                
                AddMob<HarpyMachine>(ID.Harpy);
                loot = new (ID.Harpy);
                loot.Add(1, 2, ID.Foul);  
                loot.Add(0.5f, 1, ID.Wool);  

                AddMob<BearMachine>(ID.Bear);
                loot = new (ID.Bear);
                loot.Add(1, 2, ID.Meat);
                loot.Add(0.5f, 1, ID.Meat);
                loot.Add(0.4f, 1, ID.Foul);

                AddMob<WatchdogMachine>(ID.Watchdog);
                loot = new (ID.Watchdog);
                loot.Add(1, 2, ID.Meat);
                loot.Add(0.5f, 1, ID.Meat);
                loot.Add(0.4f, 1, ID.Foul);

                AddMob<SpiderMachine>(ID.Spider);
                loot = new (ID.Spider);
                loot.Add(1, 2, ID.SpiderWeb);   

                AddMob<ViperMachine>(ID.Viper);
                loot = new (ID.Viper);
                loot.Add(1, 2, ID.SpiderWeb);
                loot.Add(0.5f, 1, ID.Blood);

                AddMob<SawbladeMachine>(ID.Sawblade);
                loot = new (ID.Sawblade);
                loot.Add(1, 1, ID.Steel);
                loot.Add(0.5f, 1, ID.Slag);

                AddMob<BallistaMachine>(ID.Ballista);
                loot = new (ID.Ballista);
                loot.Add(1, 1, ID.Steel);
                loot.Add(0.5f, 1, ID.Slag);

                AddMob<TurretMachine>(ID.Turret);
                loot = new (ID.Turret);
                loot.Add(0.7f, 5, ID.Bullet);
                loot.Add(0.1f, 1, ID.Pistol);

                AddMob<LichMachine>(ID.Lich);
                loot = new (ID.Lich);
                loot.Add(1, 3, ID.DiamondAxe);
                loot.Add(0.5f, 2, ID.Meat);
                loot.Add(0.3f, 1, ID.OldRadio);   

                AddMob<TreeMimicMachine>(ID.TreeMimic);
                loot = new (ID.TreeMimic);
                loot.Add(1, 2, ID.Log);
                loot.Add(0.5f, 1, ID.Log);
                loot.Add(0.4f, 1, ID.Acorn);
                loot.Add(0.3f, 1, ID.Sticks);
                loot.Add(0.5f, 1, ID.MourningWood);

                AddMob<MannequinMachine>(ID.Mannequin);
                loot = new (ID.Mannequin);
                loot.Add(1, 1, ID.Foul);
                loot.Add(0.5f, 1, ID.Foul);
                loot.Add(0.3f, 1, ID.Fabric);

                AddMob<VisitorMachine>(ID.Visitor);
                loot = new (ID.Visitor); // the visitor drops nothing

                AddMob<VampireMachine>(ID.Vampire);
                loot = new (ID.Vampire);
                loot.Add(0.3f, 1, ID.Blood);
                loot.Add(0.1f, 1, ID.Foul);

                AddMob<DemonEyeMachine>(ID.DemonEye);
                loot = new (ID.DemonEye);
                loot.Add(1, 1, ID.Foul);
                loot.Add(0.3f, 1, ID.Cytoplasm);

                AddMob<HornetMachine>(ID.Hornet);
                loot = new (ID.Hornet); // the hornet drops nothing

                AddMob<PigeonMachine>(ID.Pigeon);
                loot = new (ID.Pigeon); // the pigeon drops nothing

                AddMob<GnomeMachine>(ID.Gnome);
                loot = new (ID.Gnome); // the gnome drops its stolen items on death

                AddMob<RatMachine>(ID.Rat);
                loot = new (ID.Rat); // the rat drops nothing

                AddMob<CongregantMachine>(ID.Congregant);
                loot = new (ID.Congregant);
                loot.Add(0.5f, 1, ID.CrudeHatchet);
                loot.Add(0.3f, 1, ID.Foul);

                AddMob<AcolyteMachine>(ID.Acolyte);
                loot = new (ID.Acolyte);
                loot.Add(0.5f, 1, ID.Torch);
                loot.Add(0.3f, 1, ID.Foul);

                AddMob<HereticMachine>(ID.Heretic);
                loot = new (ID.Heretic);
                loot.Add(0.5f, 1, ID.Dagger);
                loot.Add(0.3f, 1, ID.Foul);

                AddMob<CultistMachine>(ID.Cultist);
                loot = new (ID.Cultist);
                loot.Add(0.5f, 1, ID.Foul);
                loot.Add(0.3f, 1, ID.Blood);

                AddMob<ThrallMachine>(ID.Thrall);
                loot = new (ID.Thrall); // the thrall drops nothing
                  
                Dictionary.Add(ID.ItemPrefab, new Entity
                {
                        Bounds = Vector3Int.zero,
                        Collision = Main.IndexNoCollide,
                        PrefabName = ID.ItemPrefab,
                        Machine = typeof(ItemMachine),
                        StaticLoad = false,
                        SpawnOffset = MidAir
                });
                 
        }

        private static void AddMob<T>(ID id) where T : EntityMachine
        {
                Dictionary.Add(id, new Entity
                {
                        Bounds = Vector3.one * 0.7f,
                        Collision = Main.IndexSemiCollide,
                        PrefabName = ID.MobPrefab,
                        Machine = typeof(T),
                        StaticLoad = false,
                        SpawnOffset = MidAir,
                });
        }

        private static void AddStructure<T>(ID id, Vector3Int bounds, int collision, byte navValue = NavMap.Block) where T : EntityMachine
        {
                Dictionary.Add(id, new Entity
                {
                        Bounds = bounds,
                        Collision = collision,
                        PrefabName = ID.StructurePrefab,
                        Machine = typeof(T),
                        StaticLoad = true,
                        SpawnOffset = Floor,
                        NavValue = navValue,
                });
        }
 
        public static void AddBlock(ID id)
        {
                Dictionary.Add(id, id == ID.MiningBox ? MiningBoxEntity : Block); 
        }

        public static void SpawnItem(ID id, Vector3 worldPosition, int amount = 1, bool stackOnSpawn = true, Vector3 velocity = default, int despawn = -1)
        {
                SpawnItem(new ItemSlot(id, amount), worldPosition, stackOnSpawn, amount, velocity, despawn);
        }

        public static ItemInfo SpawnItem(ItemSlot slot, Vector3 worldPosition, bool stackOnSpawn = true, int amount = 999, Vector3 velocity = default, int despawn = -1)
        {  
                Entity entity = Dictionary[ID.ItemPrefab];
                int target = slot.Stack - amount;
                ItemInfo spawned = null;
                while (slot.Stack != target && !slot.isEmpty())
                {
                        GameObject gameObject = ObjectPool.GetObject(ID.ItemPrefab);
                        gameObject.transform.position = Vector3Int.FloorToInt(worldPosition) + entity.SpawnOffset;

                        EntityMachine currentEntityMachine = 
                                gameObject.GetComponent<EntityMachine>() ?? gameObject.AddComponent<ItemMachine>();
                        EntityItemLoad.InviteEntity(currentEntityMachine);

                        ItemInfo itemInfo = (ItemInfo)CreateInfo(ID.ItemPrefab, worldPosition);
                        itemInfo.item = new ItemSlot();
                        itemInfo.item.Add(slot, slot.Stack - target);
                        itemInfo.Velocity = velocity;
                        itemInfo.despawn = despawn;
                        
                        itemInfo.StackOnSpawn = stackOnSpawn;
                        currentEntityMachine.Initialize(itemInfo);
                        if (Helper.IsHost())
                            ItemSync.BroadcastSpawn(itemInfo);
                        spawned = itemInfo;
                }
                return spawned;
        }

        
        public static Info Spawn(ID id, Vector3Int worldPosition)
        {
                Entity entity = Dictionary[id];
                GameObject gameObject = ObjectPool.GetObject(entity.PrefabName, id);
                gameObject.transform.position = worldPosition + entity.SpawnOffset;   
        
                EntityMachine currentEntityMachine = (EntityMachine)
                        (gameObject.GetComponent<EntityMachine>() ?? gameObject.AddComponent(entity.Machine));

                Info info = CreateInfo(id, worldPosition);
                if (entity.StaticLoad)
                {
                        EntityStaticLoad.InviteEntity(currentEntityMachine, entity);
                        // Persist the placed structure into its chunk so it survives
                        // reloads and shows up on the map (markers read chunk.StaticEntity).
                        World.Inst[World.GetChunkCoordinate(worldPosition)].StaticEntity.Add(info);
                        if (World.Inst.Map != null)
                        {
                                World.Inst.Map.Dirty = true;
                                World.Inst.Map.ResetMarkers();
                        }
                }
                else
                        EntityDynamicLoad.InviteEntity(currentEntityMachine);
                currentEntityMachine.Initialize(info);
                if (Helper.IsHost() && entity.StaticLoad)
                        EntitySync.BroadcastEntitySpawn(info);
                return info;
        } 

        public static EntityMachine SpawnFromInfo(Info info, bool invite = false)
        {
                Entity entity = Dictionary[info.id];
                GameObject gameObject = entity.PrefabName == ID.ItemPrefab
                        ? ObjectPool.GetObject(entity.PrefabName)
                        : ObjectPool.GetObject(entity.PrefabName, info.id);
                gameObject.transform.position = info.position;

                EntityMachine currentEntityMachine = (EntityMachine)
                        (gameObject.GetComponent<EntityMachine>() ?? gameObject.AddComponent(entity.Machine));

                if (entity.StaticLoad)
                        EntityStaticLoad.InviteEntity(currentEntityMachine, entity);
                else if (invite)
                {
                        if (info is ItemInfo)
                        {
                                EntityItemLoad.InviteEntity(currentEntityMachine);
                                currentEntityMachine.Initialize(info);
                                if (Helper.IsHost())
                                    ItemSync.BroadcastSpawn(info);
                                return currentEntityMachine;
                        }
                        else
                                EntityDynamicLoad.InviteEntity(currentEntityMachine);
                }

                currentEntityMachine.Initialize(info);
                return currentEntityMachine;
        }

        public static Info CreateInfo(ID id, Vector3 worldPosition)
        {
                if (!Dictionary.ContainsKey(id)) //TODO
                {
                        Entity entity = Dictionary[ID.ItemPrefab];

                        MethodInfo method = entity.Machine.GetMethod("CreateInfo", BindingFlags.Public | BindingFlags.Static);

                        if (method != null && method.ReturnType == typeof(Info))
                        {
                                ItemInfo info = (ItemInfo)method.Invoke(null, null);
                                info.id = ID.ItemPrefab;
                                info.item = new ItemSlot(id);
                                info.position = worldPosition + entity.SpawnOffset;
                                return info;
                        }
                }
                else
                {
                        Entity entity = Dictionary[id];

                        MethodInfo method = entity.Machine.GetMethod("CreateInfo", BindingFlags.Public | BindingFlags.Static);

                        if (method != null && method.ReturnType == typeof(Info))
                        {
                                Info info = (Info)method.Invoke(null, null);
                                info.id = id;
                                info.position = worldPosition + entity.SpawnOffset;
                                return info;
                        }
                } 
                Debug.Log("error making info");
                return null;
        }

}