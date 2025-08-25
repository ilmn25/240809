using UnityEngine;

public class Particle
{
    private static int _gibsCount = 0;
    public static readonly int MaxGibs = 100;
    
    public static void Spawn(ID id, Vector3 position, Quaternion rotation, bool force = false)
    {
        if (force || _gibsCount < MaxGibs)
        {
            GameObject obj = ObjectPool.GetObject(id);
            obj.SetActive(true);
            Debug.Log(rotation);
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            if (!force) _gibsCount++;
        }
    }
}
