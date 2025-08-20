using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
public partial class Entity
{ 
        public static readonly Dictionary<ID, Entity> Dictionary = new Dictionary<ID, Entity>();
        
        public Vector3Int Bounds;
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
                AddStructure<TreeMachine>(ID.Tree, new Vector3Int(1, 3, 1), Game.IndexCollide);
                AddStructure<WorkbenchMachine>(ID.Workbench, Vector3Int.one, Game.IndexCollide);
                AddStructure<FurnaceMachine>(ID.Furnace, Vector3Int.one, Game.IndexCollide);
                AddStructure<StonecutterMachine>(ID.Stonecutter, Vector3Int.one, Game.IndexCollide);
                AddStructure<CampfireMachine>(ID.Campfire, Vector3Int.one, Game.IndexCollide);
                AddStructure<SawmillMachine>(ID.Sawmill, Vector3Int.one, Game.IndexCollide);
                AddStructure<AnvilMachine>(ID.Anvil, Vector3Int.one, Game.IndexCollide);
                AddStructure<ConstructionMachine>(ID.Construction, Vector3Int.one, Game.IndexCollide);
                AddStructure<StationMachine>(ID.Station, Vector3Int.one, Game.IndexCollide);
                AddStructure<BasicChestMachine>(ID.Chest, Vector3Int.one, Game.IndexCollide);
                AddStructure<DecorMachine>(ID.Bush, Vector3Int.zero, Game.IndexNoCollide);
                AddStructure<DecorMachine>(ID.Grass, Vector3Int.zero, Game.IndexNoCollide); 
                AddStructure<SlabMachine>(ID.Slab, Vector3Int.one, Game.IndexNoCollide);
                
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
                
                Dictionary.Add(ID.Player, new Entity
                {
                        Bounds = Vector3Int.one,
                        Collision = Game.IndexSemiCollide,
                        PrefabName = ID.PlayerPrefab,
                        Machine = typeof(PlayerMachine),
                        StaticLoad = false,
                        SpawnOffset = MidAir,
                });
                AddMob<HunterMachine>(ID.Chito); 
                AddMob<HunterMachine>(ID.Yuuri);
                AddMob<BugMachine>(ID.SnareFlea); 
                AddMob<GhoulMachine>(ID.Megumin);
        }

        private static void AddMob<T>(ID stringID) where T : EntityMachine
        {
                Dictionary.Add(stringID, new Entity
                {
                        Bounds = Vector3Int.one,
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
        
        public static void SpawnItem(ID id, Vector3 worldPosition, int count = 1)
        {
                int stackSize = global::Item.GetItem(id).StackSize;

                while (count > 0)
                {
                        int spawnCount = Mathf.Min(count, stackSize);
                        count -= spawnCount;

                        GameObject gameObject = ObjectPool.GetObject(ID.ItemPrefab);
                        gameObject.transform.position = Vector3Int.FloorToInt(worldPosition) + Item.SpawnOffset;

                        EntityMachine currentEntityMachine = (EntityMachine)
                                (gameObject.GetComponent<EntityMachine>() ?? gameObject.AddComponent(Dictionary[id].Machine));
                        EntityDynamicLoad.InviteEntity(currentEntityMachine);

                        ItemInfo itemInfo = (ItemInfo)CreateInfo(id, worldPosition + Item.SpawnOffset);
                        itemInfo.item = new ItemSlot
                        {
                                ID = id,
                                Stack = spawnCount,
                        };

                        currentEntityMachine.Initialize(itemInfo);
                }
        }

        
        public static Info Spawn(ID stringID, Vector3 worldPosition)
        {
                GameObject gameObject = ObjectPool.GetObject(Dictionary[stringID].PrefabName, stringID);
                gameObject.transform.position = worldPosition + Dictionary[stringID].SpawnOffset;   
        
                EntityMachine currentEntityMachine = (EntityMachine)
                        (gameObject.GetComponent<EntityMachine>() ?? gameObject.AddComponent(Dictionary[stringID].Machine));
                
                if (Dictionary[stringID].StaticLoad)
                        EntityStaticLoad.InviteEntity(currentEntityMachine); 
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
                        info.stringID = stringID;
                        info.position = worldPosition + entity.SpawnOffset;
                        return info;
                }
                
                Debug.Log("error making info");
                return null;
        }
}