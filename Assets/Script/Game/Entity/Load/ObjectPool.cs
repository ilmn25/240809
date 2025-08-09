using UnityEngine;
using System.Collections.Generic;

public class ObjectPool
{
    private static Dictionary<string, Queue<GameObject>> _pools = new Dictionary<string, Queue<GameObject>>();
    
    public static GameObject GetObject(string prefabName, string stringID = null)
    {
        GameObject obj;  
        stringID ??= prefabName;
        if (_pools.ContainsKey(stringID) && _pools[stringID].Count > 0)
        { 
            obj = _pools[stringID].Dequeue();
            obj.SetActive(true);
        }
        else
        {
            obj = Object.Instantiate(Resources.Load<GameObject>($"prefab/{prefabName}"));
            obj.name = stringID;
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