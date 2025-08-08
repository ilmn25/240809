using System.Collections.Generic;
using UnityEngine;


public enum EntityType { Item, Static, Rigid }
public partial class Entity
{ 
        Vector3Int Bounds { get; }
        EntityType Type { get; }

}
public partial class Entity 
{
        public static readonly Dictionary<string, IEntity> Dictionary = new Dictionary<string, IEntity>();

        static Entity()
        {
                Dictionary.Add("tree", new Entity<ChunkEntityData>(new Vector3Int(1, 3, 1)));
                Dictionary.Add("bush1", Entity<ChunkEntityData>.Zero);
                Dictionary.Add("grass", Entity<ChunkEntityData>.Zero);
                Dictionary.Add("stage_hand", Entity<ChunkEntityData>.One);
                Dictionary.Add("slab", Entity<ChunkEntityData>.Zero);
                Dictionary.Add("snare_flea", new Entity<ChunkEntityData>(type: EntityType.Rigid));
                Dictionary.Add("chito", new Entity<ChunkEntityData>(type: EntityType.Rigid));
                Dictionary.Add("megumin", new Entity<ChunkEntityData>(type: EntityType.Rigid));
                Dictionary.Add("yuuri", new Entity<ChunkEntityData>(type: EntityType.Rigid));
        }

        public static void AddItem(string stringID)
        {
                Dictionary.Add(stringID, Entity<ChunkEntityData>.Item);
        }
        
        public static ChunkEntityData GetChunkEntityData(string stringID, Vector3Int worldPosition)
        {
                return Dictionary[stringID].GetChunkEntityData(stringID, new SVector3Int(worldPosition));
        }
        public static ChunkEntityData GetChunkEntityData(string stringID, SVector3Int worldPosition)
        {
                return Dictionary[stringID].GetChunkEntityData(stringID, worldPosition);
        }
        
        public static void SpawnItem(string stringID, Vector3Int worldPosition, bool pickUp = true, int count = 1)
        {
                for (int i = 0; i < count; i++)
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