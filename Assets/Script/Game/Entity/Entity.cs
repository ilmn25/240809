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
        
        public static Vector3 MidAir = new Vector3(0.5f, 0.3f, 0.5f);
        public static Vector3 Floor = new Vector3(0.5f, 0f, 0.5f);
         
        private static readonly Entity Item = new Entity
        {
                Bounds = Vector3Int.zero,
                Collision = Game.IndexNoCollide,
                PrefabName = ID.ItemPrefab,
                Machine = typeof(ItemMachine),
                StaticLoad = false,
                SpawnOffset = MidAir
        };  
        
        public static void Initialize()
        {
                Loot loot;
                AddStructure<TreeMachine>(ID.Tree, new Vector3Int(1, 3, 1), Game.IndexCollide);
                loot = new (ID.Tree);
                loot.Add(1, 4, ID.Log);
                loot.Add(0.5f, 1, ID.Log); 
                loot.Add(0.5f, 1, ID.Acorn); 
                
                AddStructure<SlabMachine>(ID.Slab, Vector3Int.one, Game.IndexCollide);
                loot = new (ID.Slab);
                loot.Add(1, 2, ID.Flint); 
                loot.Add(0.5f, 1, ID.Gravel);
                loot.Add(0.5f, 1, ID.Flint);
                loot.Add(0.5f, 1, ID.Flint);
                
                AddStructure<WorkbenchMachine>(ID.Workbench, Vector3Int.one, Game.IndexCollide);
                AddStructure<FurnaceMachine>(ID.Furnace, Vector3Int.one, Game.IndexCollide);
                AddStructure<StonecutterMachine>(ID.Stonecutter, Vector3Int.one, Game.IndexCollide);
                AddStructure<CampfireMachine>(ID.Campfire, Vector3Int.one, Game.IndexCollide);
                AddStructure<SawmillMachine>(ID.Sawmill, Vector3Int.one, Game.IndexCollide);
                AddStructure<AnvilMachine>(ID.Anvil, Vector3Int.one, Game.IndexCollide);
                AddStructure<ConstructionMachine>(ID.Construction, Vector3Int.one, Game.IndexCollide);
                AddStructure<StationMachine>(ID.Station, Vector3Int.one, Game.IndexCollide);
                
                AddStructure<BasicChestMachine>(ID.Chest, Vector3Int.one, Game.IndexCollide);
                loot = new (ID.Chest);
                loot.Add(1, 1, ID.MetalChunks);
                loot.Add(1, 3, ID.Brick); 
                loot.Add(0.7f, 1, ID.Charcoal, ID.Flint);
                loot.Add(1, 1, ID.Spear, ID.StoneHatchet); 
                
                AddStructure<DecorMachine>(ID.Bush, Vector3Int.zero, Game.IndexNoCollide);
                AddStructure<DecorMachine>(ID.Grass, Vector3Int.zero, Game.IndexNoCollide);   
                AddStructure<BedMachine>(ID.Bed, Vector3Int.one, Game.IndexSemiCollide);
                AddStructure<SignMachine>(ID.Sign, Vector3Int.one, Game.IndexSemiCollide);
                AddStructure<PortalMachine>(ID.Portal, Vector3Int.one, Game.IndexSemiCollide);
                AddStructure<DecorMachine>(ID.Table, Vector3Int.one, Game.IndexCollide); 
                 
                AddMob<PlayerMachine>(ID.Player);
                
                AddMob<HunterMachine>(ID.Chito); 
                loot = new (ID.Chito);
                loot.Add(0.7f, 5, ID.Bullet); 
                loot.Add(0.1f, 1, ID.Pistol); 
                
                AddMob<HunterMachine>(ID.Yuuri);
                loot = new (ID.Yuuri);
                loot.Add(0.7f, 5, ID.Bullet);  
                loot.Add(0.1f, 1, ID.Pistol);  
                
                AddMob<SheepMachine>(ID.Sheep);
                loot = new (ID.Sheep);
                loot.Add(1, 1, ID.Meat);  
                loot.Add(0.5f, 1, ID.Meat);  
                loot.Add(0.5f, 1, ID.Wool); 
                loot.Add(0.5f, 1, ID.Wool); 
                loot.Add(0.5f, 1, ID.Wool); 
                
                AddMob<BugMachine>(ID.SnareFlea); 
                loot = new (ID.SnareFlea);
                loot.Add(0.5f, 6, ID.Sticks); 
                
                AddMob<GhoulMachine>(ID.Megumin);
                loot = new (ID.Megumin);
                loot.Add(0.1f, 1, ID.SteelSword, ID.DiamondAxe); 
                
                AddMob<SlimeMachine>(ID.Slime);
                loot =  new (ID.Slime);
                loot.Add(1, 2, ID.Cytoplasm); 
                
                AddMob<HarpyMachine>(ID.Harpy);
                loot = new (ID.Harpy);
                loot.Add(1, 2, ID.Chicken);  
                loot.Add(0.5f, 1, ID.Wool);   
                 
                Dictionary.Add(ID.Block, new Entity
                {
                        Bounds = Vector3Int.one,
                        Collision = Game.IndexSemiCollide,
                        PrefabName = ID.BlockPrefab,
                        Machine = typeof(BlockMachine),
                        StaticLoad = true,
                        SpawnOffset = Floor,
                });
                Dictionary.Add(ID.BreakBlock, new Entity
                {
                        Bounds = Vector3Int.one,
                        Collision = Game.IndexNoCollide,
                        PrefabName = ID.BlockPrefab,
                        Machine = typeof(BreakBlockMachine),
                        StaticLoad = true,
                        SpawnOffset = Floor,
                });
                 
        }

        private static void AddMob<T>(ID stringID) where T : EntityMachine
        {
                Dictionary.Add(stringID, new Entity
                {
                        Bounds = Vector3.one * 0.7f,
                        Collision = Game.IndexSemiCollide,
                        PrefabName = ID.MobPrefab,
                        Machine = typeof(T),
                        StaticLoad = false,
                        SpawnOffset = MidAir,
                });
        }

        private static void AddStructure<T>(ID stringID, Vector3Int bounds, int collision) where T : EntityMachine
        {
                Dictionary.Add(stringID, new Entity
                {
                        Bounds = bounds,
                        Collision = collision,
                        PrefabName = ID.StructurePrefab,
                        Machine = typeof(T),
                        StaticLoad = true,
                        SpawnOffset = Floor,
                });
        }

        public static void AddItem(ID stringID)
        {
                Dictionary.Add(stringID, Item); 
        }

        public static void SpawnItem(ID id, Vector3 worldPosition, int amount = 1, bool stackOnSpawn = true, Vector3 velocity = default, int despawn = -1)
        {
                SpawnItem(new ItemSlot(id, amount), worldPosition, stackOnSpawn, amount, velocity, despawn);
        }

        public static void SpawnItem(ItemSlot slot, Vector3 worldPosition, bool stackOnSpawn = true, int amount = 999, Vector3 velocity = default, int despawn = -1) // amount to add, add all 999
        {  
                int target = slot.Stack - amount;
                while (slot.Stack != target && !slot.isEmpty())
                {
                        GameObject gameObject = ObjectPool.GetObject(ID.ItemPrefab);
                        gameObject.transform.position = Vector3Int.FloorToInt(worldPosition) + Item.SpawnOffset;

                        EntityMachine currentEntityMachine = 
                                (gameObject.GetComponent<EntityMachine>() ?? gameObject.AddComponent<ItemMachine>());
                        EntityDynamicLoad.InviteEntity(currentEntityMachine);

                        ItemInfo itemInfo = (ItemInfo)CreateInfo(slot.ID, worldPosition + Item.SpawnOffset);
                        itemInfo.item = new ItemSlot();
                        itemInfo.item.Add(slot, slot.Stack - target);
                        itemInfo.Velocity = velocity;
                        itemInfo.despawn = despawn;
                        
                        itemInfo.StackOnSpawn = stackOnSpawn;
                        currentEntityMachine.Initialize(itemInfo);
                }
        }

        
        public static Info Spawn(ID stringID, Vector3 worldPosition)
        {
                Entity entity = Dictionary[stringID];
                GameObject gameObject = ObjectPool.GetObject(entity.PrefabName, stringID);
                gameObject.transform.position = worldPosition + entity.SpawnOffset;   
        
                EntityMachine currentEntityMachine = (EntityMachine)
                        (gameObject.GetComponent<EntityMachine>() ?? gameObject.AddComponent(entity.Machine));

                if (entity.StaticLoad)
                        EntityStaticLoad.InviteEntity(currentEntityMachine, entity); 
                else
                        EntityDynamicLoad.InviteEntity(currentEntityMachine);
                Info info = CreateInfo(stringID, worldPosition);
                currentEntityMachine.Initialize(info);
                return info;
        }

        public static Info CreateInfo(ID stringID, Vector3 worldPosition)
        {
                Entity entity = Dictionary[stringID];

                MethodInfo method = entity.Machine.GetMethod("CreateInfo", BindingFlags.Public | BindingFlags.Static);

                if (method != null && method.ReturnType == typeof(Info))
                {
                        Info info = (Info)method.Invoke(null, null);
                        info.id = stringID;
                        info.position = worldPosition + entity.SpawnOffset;
                        return info;
                }
                
                Debug.Log("error making info");
                return null;
        }
}