using System;
using System.Collections.Generic;
using UnityEngine;

public partial class Entity
{ 
        public static readonly Dictionary<string, Entity> Dictionary = new Dictionary<string, Entity>();
        
        public Vector3Int Bounds;
        public string PrefabName;
        public Type Machine;
        public bool Collision;
        public bool StaticLoad;
         
        private static readonly Entity Item = new Entity
        {
                Bounds = Vector3Int.one,
                Collision = false,
                PrefabName = "item",
                Machine = typeof(ItemMachine),
                StaticLoad = false
        };  
        
        public static void Initialize()
        {
                AddStructure<TreeMachine>("tree", new Vector3Int(1, 3, 1), true);
                AddStructure<WorkBenchMachine>("workbench", Vector3Int.one, true);
                AddStructure<StationMachine>("station", Vector3Int.one, true);
                AddStructure<ChestMachine>("chest", Vector3Int.one, true);
                AddStructure<DecorMachine>("bush1", Vector3Int.zero, false);
                AddStructure<DecorMachine>("grass", Vector3Int.zero, false); 
                AddStructure<SlabMachine>("slab", Vector3Int.one, false);
                
                AddMob<HunterMachine>("chito"); 
                AddMob<HunterMachine>("yuuri");
                AddMob<BugMachine>("snare_flea"); 
                AddMob<GhoulMachine>("megumin");
        }

        private static void AddMob<T>(string stringID) where T : EntityMachine
        {
                Dictionary.Add(stringID, new Entity
                {
                        Bounds = Vector3Int.zero,
                        Collision = false,
                        PrefabName = "mob",
                        Machine = typeof(T),
                        StaticLoad = false,
                });
        }

        private static void AddStructure<T>(string stringID, Vector3Int bounds, bool collision) where T : EntityMachine
        {
                Dictionary.Add(stringID, new Entity
                {
                        Bounds = bounds,
                        Collision = collision,
                        PrefabName = "structure",
                        Machine = typeof(T),
                        StaticLoad = true,
                });
        }

        public static void AddItem(string stringID)
        {
                Dictionary.Add(stringID, Item); 
        }
        
        public static ChunkEntityData GetChunkEntityData(string stringID, Vector3Int worldPosition)
        {
                return new ChunkEntityData()
                {
                        stringID = stringID,
                        position = new SVector3Int(worldPosition),
                };
        }
        public static ChunkEntityData GetChunkEntityData(string stringID, SVector3Int worldPosition)
        {
                return new ChunkEntityData()
                {
                        stringID = stringID,
                        position = worldPosition,
                }; 
        }
        
        public static void SpawnItem(string stringID, Vector3Int worldPosition, int count = 1)
        {
                for (int i = 0; i < count; i++)
                {
                        ChunkEntityData entityData = GetChunkEntityData(stringID, worldPosition);

                        GameObject gameObject = ObjectPool.GetObject("item");
                        gameObject.transform.position = worldPosition + new Vector3(0.5f, 0.5f, 0.5f); 
        
                        EntityMachine currentEntityMachine = (EntityMachine)
                                (gameObject.GetComponent<EntityMachine>() ?? gameObject.AddComponent(Dictionary[stringID].Machine));
                        EntityDynamicLoad.InviteEntity(currentEntityMachine); 
                        currentEntityMachine.Initialize(entityData);
                        
                } 
        }
        
        public static void Spawn(string stringID, Vector3Int worldPosition)
        {
                ChunkEntityData entityData = GetChunkEntityData(stringID, worldPosition);
                
                GameObject gameObject = ObjectPool.GetObject(Dictionary[stringID].PrefabName, stringID);
                gameObject.transform.position = worldPosition + new Vector3(0.5f, 0, 0.5f);   
        
                EntityMachine currentEntityMachine = (EntityMachine)
                        (gameObject.GetComponent<EntityMachine>() ?? gameObject.AddComponent(Dictionary[stringID].Machine));
                
                if (Dictionary[stringID].StaticLoad)
                        EntityStaticLoad.InviteEntity(currentEntityMachine); 
                else
                        EntityDynamicLoad.InviteEntity(currentEntityMachine); 
                currentEntityMachine.Initialize(entityData);
        }
}