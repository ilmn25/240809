using UnityEngine;

public class Entity
{
        public static void SpawnItem(string stringID, Vector3 worldPosition)
        {
                EntityData entityData = new EntityData(stringID, new SerializableVector3(worldPosition), type: EntityType.Item);
        
                GameObject gameObject = EntityPoolSingleton.Instance.GetObject("item");
                gameObject.transform.position = worldPosition;
                gameObject.GetComponent<SpriteRenderer>().sprite = 
                        Resources.Load<Sprite>($"texture/sprite/{stringID}"); 
        
                EntityHandler currentEntityHandler = gameObject.GetComponent<EntityHandler>();
                EntityDynamicLoadSingleton._entityList.Add(currentEntityHandler); 
                currentEntityHandler.Initialize(entityData, false);
         
        }
        public static void SpawnPrefab(string stringID, Vector3 worldPosition)
        {
                EntityData entityData = new EntityData(stringID, new SerializableVector3(worldPosition), type: EntityType.Rigid);
                GameObject gameObject = EntityPoolSingleton.Instance.GetObject(stringID);
                gameObject.transform.position = worldPosition;   
        
                EntityHandler currentEntityHandler = gameObject.GetComponent<EntityHandler>();
                EntityDynamicLoadSingleton._entityList.Add(currentEntityHandler); 
                currentEntityHandler.Initialize(entityData, false);
         
        }
}