using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class EntityPoolStatic : MonoBehaviour
{
    public static EntityPoolStatic Instance { get; private set; }
    private Dictionary<string, Queue<GameObject>> _pools = new Dictionary<string, Queue<GameObject>>();

    void Awake()
    {
        Instance = this; 
    }
 
    
    public GameObject GetObject(string prefabName)
    {
        
        GameObject obj;
         
        if (_pools.ContainsKey(prefabName) && _pools[prefabName].Count > 0)
        {
            obj = _pools[prefabName].Dequeue();
            obj.SetActive(true);
        }
        else
        {
            obj = Instantiate(Resources.Load<GameObject>($"prefab/{prefabName}"));
            obj.name = prefabName;
            obj.AddComponent<EntityHandler>();
        } 

        return obj;
    }

    public void ReturnObject(GameObject obj)
    {
        string prefabName = obj.name;
        obj.SetActive(false);

        if (!_pools.ContainsKey(prefabName))
        {
            _pools[prefabName] = new Queue<GameObject>();
        }

        _pools[prefabName].Enqueue(obj);
    }
}