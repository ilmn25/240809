using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class ObjectPool
{
    private static Dictionary<string, Queue<GameObject>> _pools = new Dictionary<string, Queue<GameObject>>();
    
    public static GameObject GetObject(string prefabName)
    {
        GameObject obj;
         
        if (_pools.ContainsKey(prefabName) && _pools[prefabName].Count > 0)
        {
            obj = _pools[prefabName].Dequeue();
            obj.SetActive(true);
        }
        else
        {
            obj = Object.Instantiate(Resources.Load<GameObject>($"prefab/{prefabName}"));
            obj.name = prefabName;
        } 

        return obj;
    }

    public static void ReturnObject(GameObject obj)
    { 
        obj.SetActive(false);
        
        if (!_pools.ContainsKey(obj.name))
            _pools[obj.name] = new Queue<GameObject>();

        _pools[obj.name].Enqueue(obj);
    }
}