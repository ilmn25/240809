using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class Utility
{
    private static readonly string SavePath;
    private static readonly BinaryFormatter BinaryFormatter = new BinaryFormatter();

    static Utility()
    {
        SavePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads\\";
    }
    public static Vector3 AddToVector(Vector3 vector, float x, float y, float z)
    {
        return new Vector3(vector.x + x, vector.y + y, vector.z + z);
    }

    public static Vector3Int AddToVector(Vector3Int vector, int x, int y, int z)
    {
        return new Vector3Int(vector.x + x, vector.y + y, vector.z + z);
    }

    public static float SquaredDistance(Vector3 a, Vector3 b)
    {
        return (a.x - b.x) * (a.x - b.x) + 
               (a.y - b.y) * (a.y - b.y) + 
               (a.z - b.z) * (a.z - b.z);
    } 
    public static void Log(params object[] parameters)
    {
        if (parameters.Length == 0)
        {
            Debug.Log("(uwu) HAI");
        }
        else
        {
            string logMessage = string.Join(" | ", parameters);
            Debug.Log(logMessage);
        }
    }

    public static bool isLayer(int colliderIndex, int targetIndex)
    {
        return (colliderIndex & (1 << targetIndex)) != 0;
    }

    public static float GetDeltaTime()
    {
        return (Time.deltaTime < Game.MaxDeltaTime) ? Time.deltaTime : Game.MaxDeltaTime;
    }
 

    public static void Save<T>(T data, string filePath)
    {
        using (FileStream file = File.Create(SavePath + filePath + ".dat"))
        {
            BinaryFormatter.Serialize(file, data);
        }
    }

    public static T Load<T>(string filePath)
    {
        if (File.Exists(SavePath + filePath + ".dat"))
        {
            using (FileStream file = File.Open(SavePath + filePath + ".dat", FileMode.Open))
            {
                return (T)BinaryFormatter.Deserialize(file);
            }
        }
        // Debug.LogWarning("File " + filePath + " does not exist");
        return default;
    }

    public static bool IsInLayerMask(GameObject obj, LayerMask mask)
    {
        return ((1 << obj.layer) & mask) != 0;
    }
}
