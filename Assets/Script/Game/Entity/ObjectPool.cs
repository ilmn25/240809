using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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
        if (obj == null) return;
        obj.SetActive(false);
        
        if (!_pools.ContainsKey(obj.name))
            _pools[obj.name] = new Queue<GameObject>();

        if (_pools[obj.name].Contains(obj))
            return;

        _pools[obj.name].Enqueue(obj);
    }
}