using System.Collections.Generic;
using UnityEngine;

public class EntitySingleton : MonoBehaviour
{
        public static Dictionary<string, EntityData> dictionary = new Dictionary<string, EntityData>();

        private void Awake()
        {
                dictionary.Add("tree", new EntityData( new Vector3Int(1, 3, 1)));
                dictionary.Add("bush1", EntityData.Zero);
                dictionary.Add("grass", EntityData.Zero);
                dictionary.Add("stage_hand", EntityData.One);
                dictionary.Add("slab", EntityData.Zero);
                dictionary.Add("snare_flea", EntityData.Rigid);
                dictionary.Add("chito", EntityData.Rigid);
                dictionary.Add("megumin", EntityData.Rigid);
                dictionary.Add("yuuri", EntityData.Rigid);
        }

        public static void AddItem(string stringID)
        {
                dictionary.Add(stringID, EntityData.Item);
        }
        
        public static void SpawnItem(string stringID, Vector3Int worldPosition, bool pickUp = true)
        {
                ChunkEntityData entityData = new ChunkEntityData(stringID, new SerializableVector3Int(worldPosition));
        
                GameObject gameObject = EntityPoolSingleton.Instance.GetObject("item");
                gameObject.transform.position = worldPosition + new Vector3(0.5f, 0.5f, 0.5f);
                gameObject.GetComponent<SpriteRenderer>().sprite = 
                        Resources.Load<Sprite>($"texture/sprite/{stringID}"); 
        
                EntityHandler currentEntityHandler = gameObject.GetComponent<EntityHandler>();
                EntityDynamicLoadSingleton._entityList.Add(currentEntityHandler); 
                currentEntityHandler.Initialize(entityData, false);
                gameObject.transform.GetComponent<ItemMachine>().pickUp = pickUp;
        }
        
        public static void SpawnPrefab(string stringID, Vector3Int worldPosition)
        {
                ChunkEntityData entityData = new ChunkEntityData(stringID, new SerializableVector3Int(worldPosition));
                GameObject gameObject = EntityPoolSingleton.Instance.GetObject(stringID);
                gameObject.transform.position = worldPosition + new Vector3(0.5f, 0.5f, 0.5f);   
        
                EntityHandler currentEntityHandler = gameObject.GetComponent<EntityHandler>();
                EntityDynamicLoadSingleton._entityList.Add(currentEntityHandler); 
                currentEntityHandler.Initialize(entityData, false);
        }
}