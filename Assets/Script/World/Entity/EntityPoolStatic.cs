using UnityEngine;
using System.Collections.Generic;

public class EntityPoolStatic : MonoBehaviour
{
    public static EntityPoolStatic Instance { get; private set; }
    private Dictionary<string, Queue<GameObject>> _pools = new Dictionary<string, Queue<GameObject>>();

    void Awake()
    {
        Instance = this; 
    }
 
    
    public GameObject GetObject(EntityData entity, string prefabName = null)
    {
        GameObject obj;
        EntityHandler entityHandler;
        if (prefabName == null) prefabName = entity.ID;
         
        if (_pools.ContainsKey(prefabName) && _pools[prefabName].Count > 0)
        {
            obj = _pools[prefabName].Dequeue();
            entityHandler = obj.AddComponent<EntityHandler>();
            entityHandler._entityData = entity;
            obj.SetActive(true);
        }
        else
        {
            obj = Instantiate(Resources.Load<GameObject>($"prefab/{prefabName}"));
            obj.transform.parent = transform;
            obj.name = prefabName;
            entityHandler = obj.AddComponent<EntityHandler>();
            entityHandler._entityData = entity;
        } 

        return obj;
    }

    public void ReturnObject(GameObject obj)
    {
        Destroy(obj.GetComponent<EntityHandler>());
        string prefabName = obj.name;
        obj.SetActive(false);

        if (!_pools.ContainsKey(prefabName))
        {
            _pools[prefabName] = new Queue<GameObject>();
        }

        _pools[prefabName].Enqueue(obj); }
}