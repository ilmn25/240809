using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
public partial class Entity
{ 
        public static readonly Dictionary<string, Entity> Dictionary = new Dictionary<string, Entity>();
        
        public Vector3Int Bounds;
        public string PrefabName;
        public Type Machine;
        public int Collision;
        public bool StaticLoad;
        public Vector3 SpawnOffset;
        
        public static Vector3 MidAir = new Vector3(0.5f, 0.3f, 0.5f);
        public static Vector3 Floor = new Vector3(0.5f, 0f, 0.5f);
         
        private static readonly Entity Item = new Entity
        {
                Bounds = Vector3Int.one,
                Collision = Game.IndexNoCollide,
                PrefabName = "item",
                Machine = typeof(ItemMachine),
                StaticLoad = false,
                SpawnOffset = MidAir
        };  
        
        public static void Initialize()
        {
                AddStructure<TreeMachine>("tree", new Vector3Int(1, 3, 1), Game.IndexCollide);
                AddStructure<WorkBenchMachine>("workbench", Vector3Int.one, Game.IndexCollide);
                AddStructure<StationMachine>("station", Vector3Int.one, Game.IndexCollide);
                AddStructure<ChestMachine>("chest", Vector3Int.one, Game.IndexCollide);
                AddStructure<DecorMachine>("bush1", Vector3Int.zero, Game.IndexNoCollide);
                AddStructure<DecorMachine>("grass", Vector3Int.zero, Game.IndexNoCollide); 
                AddStructure<SlabMachine>("slab", Vector3Int.one, Game.IndexNoCollide);
                
                Dictionary.Add("block", new Entity
                {
                        Bounds = Vector3Int.one,
                        Collision = Game.IndexSemiCollide,
                        PrefabName = "block",
                        Machine = typeof(BlockMachine),
                        StaticLoad = true,
                        SpawnOffset = Floor,
                });
                
                Dictionary.Add("player", new Entity
                {
                        Bounds = Vector3Int.one,
                        Collision = Game.IndexSemiCollide,
                        PrefabName = "player",
                        Machine = typeof(PlayerMachine),
                        StaticLoad = false,
                        SpawnOffset = MidAir,
                });
                AddMob<HunterMachine>("chito"); 
                AddMob<HunterMachine>("yuuri");
                AddMob<BugMachine>("snare_flea"); 
                AddMob<GhoulMachine>("megumin");
        }

        private static void AddMob<T>(string stringID) where T : EntityMachine
        {
                Dictionary.Add(stringID, new Entity
                {
                        Bounds = Vector3Int.one,
                        Collision = Game.IndexSemiCollide,
                        PrefabName = "mob",
                        Machine = typeof(T),
                        StaticLoad = false,
                        SpawnOffset = MidAir,
                });
        }

        private static void AddStructure<T>(string stringID, Vector3Int bounds, int collision) where T : EntityMachine
        {
                Dictionary.Add(stringID, new Entity
                {
                        Bounds = bounds,
                        Collision = collision,
                        PrefabName = "structure",
                        Machine = typeof(T),
                        StaticLoad = true,
                        SpawnOffset = Floor,
                });
        }

        public static void AddItem(string stringID)
        {
                Dictionary.Add(stringID, Item); 
        } 
        
        public static void SpawnItem(string stringID, Vector3 worldPosition, int count = 1)
        {
                for (int i = 0; i < count; i++)
                {

                        GameObject gameObject = ObjectPool.GetObject("item");
                        gameObject.transform.position = Vector3Int.FloorToInt(worldPosition) + new Vector3(0.5f, 0.5f, 0.5f); 
        
                        EntityMachine currentEntityMachine = (EntityMachine)
                                (gameObject.GetComponent<EntityMachine>() ?? gameObject.AddComponent(Dictionary[stringID].Machine));
                        EntityDynamicLoad.InviteEntity(currentEntityMachine); 
                        currentEntityMachine.Initialize(CreateInfo(stringID, worldPosition)); 
                        
                } 
        }
        
        public static void Spawn(string stringID, Vector3 worldPosition)
        {
                GameObject gameObject = ObjectPool.GetObject(Dictionary[stringID].PrefabName, stringID);
                gameObject.transform.position = worldPosition + new Vector3(0.5f, 0, 0.5f);   
        
                EntityMachine currentEntityMachine = (EntityMachine)
                        (gameObject.GetComponent<EntityMachine>() ?? gameObject.AddComponent(Dictionary[stringID].Machine));
                
                if (Dictionary[stringID].StaticLoad)
                        EntityStaticLoad.InviteEntity(currentEntityMachine); 
                else
                        EntityDynamicLoad.InviteEntity(currentEntityMachine); 
                currentEntityMachine.Initialize(CreateInfo(stringID, worldPosition));
        }

        public static Info CreateInfo(string stringID, Vector3 worldPosition)
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