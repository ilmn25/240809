using UnityEngine;
using System.Collections.Generic;

public class ObjectPool
{
    private static Dictionary<string, Queue<GameObject>> _pools = new Dictionary<string, Queue<GameObject>>();
    
    public static GameObject GetObject(ID prefabName, ID stringID = ID.Null)
    {
        GameObject obj;
        if (stringID == ID.Null) stringID = prefabName;
        if (_pools.ContainsKey(stringID.ToString()) && _pools[stringID.ToString()].Count > 0)
        { 
            obj = _pools[stringID.ToString()].Dequeue();
            obj.SetActive(true);
        }
        else
        {
            obj = Object.Instantiate(Resources.Load<GameObject>($"Prefab/{prefabName}"));
            obj.name = stringID.ToString();
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