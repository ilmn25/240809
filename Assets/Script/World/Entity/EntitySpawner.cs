using UnityEngine;

public class EntitySpawner
{
        public static void SpawnItem(string stringID, Vector3Int worldPosition, bool pickUp = true)
        {
                EntityData entityData = new EntityData(stringID, new SerializableVector3Int(worldPosition), type: EntityType.Item);
        
                GameObject gameObject = EntityPoolSingleton.Instance.GetObject("item");
                gameObject.transform.position = worldPosition + new Vector3(0.5f, 0.5f, 0.5f);
                gameObject.GetComponent<SpriteRenderer>().sprite = 
                        Resources.Load<Sprite>($"texture/sprite/{stringID}"); 
        
                EntityHandler currentEntityHandler = gameObject.GetComponent<EntityHandler>();
                EntityDynamicLoadSingleton._entityList.Add(currentEntityHandler); 
                currentEntityHandler.Initialize(entityData, false);
                gameObject.transform.GetComponent<ItemStateMachine>().pickUp = pickUp;
        }
        
        public static void SpawnPrefab(string stringID, Vector3Int worldPosition)
        {
                EntityData entityData = new EntityData(stringID, new SerializableVector3Int(worldPosition), type: EntityType.Rigid);
                GameObject gameObject = EntityPoolSingleton.Instance.GetObject(stringID);
                gameObject.transform.position = worldPosition + new Vector3(0.5f, 0.5f, 0.5f);   
        
                EntityHandler currentEntityHandler = gameObject.GetComponent<EntityHandler>();
                EntityDynamicLoadSingleton._entityList.Add(currentEntityHandler); 
                currentEntityHandler.Initialize(entityData, false);
        }
}