using System.Collections.Generic;
using UnityEngine;

public partial class Entity 
{
        public static Dictionary<string, IEntity> dictionary = new Dictionary<string, IEntity>();

        static Entity()
        {
                dictionary.Add("tree", new Entity<ChunkEntityData>(new Vector3Int(1, 3, 1)));
                dictionary.Add("bush1", Entity<ChunkEntityData>.Zero);
                dictionary.Add("grass", Entity<ChunkEntityData>.Zero);
                dictionary.Add("stage_hand", Entity<ChunkEntityData>.One);
                dictionary.Add("slab", Entity<ChunkEntityData>.Zero);
                dictionary.Add("snare_flea", new Entity<NPCCED>(type: EntityType.Rigid));
                dictionary.Add("chito", new Entity<NPCCED>(type: EntityType.Rigid));
                dictionary.Add("megumin", new Entity<NPCCED>(type: EntityType.Rigid));
                dictionary.Add("yuuri", new Entity<NPCCED>(type: EntityType.Rigid));
        }

        public static void AddItem(string stringID)
        {
                dictionary.Add(stringID, Entity<ChunkEntityData>.Item);
        }
        
        public static ChunkEntityData GetChunkEntityData(string stringID, Vector3Int worldPosition)
        {
                return dictionary[stringID].GetChunkEntityData(stringID, new SVector3Int(worldPosition));
        }
        public static ChunkEntityData GetChunkEntityData(string stringID, SVector3Int worldPosition)
        {
                return dictionary[stringID].GetChunkEntityData(stringID, worldPosition);
        }
        
        public static void SpawnItem(string stringID, Vector3Int worldPosition, bool pickUp = true)
        {
                ChunkEntityData entityData = GetChunkEntityData(stringID, worldPosition);

                GameObject gameObject = ObjectPool.GetObject("item");
                gameObject.transform.position = worldPosition + new Vector3(0.5f, 0.5f, 0.5f);
                gameObject.GetComponent<SpriteRenderer>().sprite = 
                        Resources.Load<Sprite>($"texture/sprite/{stringID}"); 
        
                EntityMachine currentEntityMachine = gameObject.GetComponent<EntityMachine>();
                EntityDynamicLoad.InviteEntity(currentEntityMachine); 
                currentEntityMachine.Initialize(entityData);
                gameObject.transform.GetComponent<ItemMachine>().pickUp = pickUp;
        }
        
        public static void SpawnPrefab(string stringID, Vector3Int worldPosition)
        {
                ChunkEntityData entityData = GetChunkEntityData(stringID, worldPosition);
                
                GameObject gameObject = ObjectPool.GetObject(stringID);
                gameObject.transform.position = worldPosition + new Vector3(0.5f, 0.5f, 0.5f);   
        
                EntityMachine currentEntityMachine = gameObject.GetComponent<EntityMachine>();
                EntityDynamicLoad.InviteEntity(currentEntityMachine); 
                currentEntityMachine.Initialize(entityData);
        }
}