using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class Helper
{
    private static readonly string SavePath = 
        System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile) + "\\Downloads\\";
    private const string SaveFormat = ".ilmn";
    public static readonly BinaryFormatter BinaryFormatter = new BinaryFormatter();
 
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
    public static float SquaredDistance(Vector3Int a, Vector3Int b)
    {
        return (a.x - b.x) * (a.x - b.x) + 
               (a.y - b.y) * (a.y - b.y) + 
               (a.z - b.z) * (a.z - b.z);
    } 
    public static float SquaredDistance(Vector3 a, Vector3Int b)
    {
        return (a.x - b.x) * (a.x - b.x) + 
               (a.y - b.y) * (a.y - b.y) + 
               (a.z - b.z) * (a.z - b.z);
    } 
    public static void Log(params object[] parameters)
    {
        if (parameters.Length == 0)
        {
            Debug.Log("LOG");
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
 
    public static Color GetColor(float r, float g, float b)
    {
        return new Color(r / 255f, g / 255f, b / 255f);
    }

    public static void FileSave<T>(T data, string filePath)
    {
        using (FileStream file = File.Create(SavePath + filePath + SaveFormat))
        {
            BinaryFormatter.Serialize(file, data);
        }
    }

    public static T FileLoad<T>(string filePath)
    {
        if (File.Exists(SavePath + filePath + SaveFormat))
        {
            using (FileStream file = File.Open(SavePath + filePath + SaveFormat, FileMode.Open))
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
    
    public static object Clone(object source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        Type type = source.GetType(); // This gets the actual subclass type
        object clone = Activator.CreateInstance(type);

        foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            if (field.IsNotSerialized) continue;
            field.SetValue(clone, field.GetValue(source));
        }

        return clone;
    }
}
