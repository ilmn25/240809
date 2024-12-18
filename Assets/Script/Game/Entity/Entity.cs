using System.Collections.Generic;
using UnityEngine;

public class Entity 
{
        public static Dictionary<string, IEntityData> dictionary = new Dictionary<string, IEntityData>();

        static Entity()
        {
                dictionary.Add("tree", new EntityData<ChunkEntityData>(new Vector3Int(1, 3, 1)));
                dictionary.Add("bush1", EntityData<ChunkEntityData>.Zero);
                dictionary.Add("grass", EntityData<ChunkEntityData>.Zero);
                dictionary.Add("stage_hand", EntityData<ChunkEntityData>.One);
                dictionary.Add("slab", EntityData<ChunkEntityData>.Zero);
                dictionary.Add("snare_flea", new EntityData<NPCCED>(type: EntityType.Rigid));
                dictionary.Add("chito", new EntityData<NPCCED>(type: EntityType.Rigid));
                dictionary.Add("megumin", new EntityData<NPCCED>(type: EntityType.Rigid));
                dictionary.Add("yuuri", new EntityData<NPCCED>(type: EntityType.Rigid));
        }

        public static void AddItem(string stringID)
        {
                dictionary.Add(stringID, EntityData<ChunkEntityData>.Item);
        }
        
        public static ChunkEntityData GetChunkEntityData(string stringID, Vector3Int worldPosition)
        {
                return dictionary[stringID].GetChunkEntityData(stringID, new SerializableVector3Int(worldPosition));
        }
        public static ChunkEntityData GetChunkEntityData(string stringID, SerializableVector3Int worldPosition)
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