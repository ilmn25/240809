using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class Helper
{
    private static readonly string SavePath = 
        System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile) + "\\Downloads\\Save\\";
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

    public static void FileSave<T>(T data, string filePath, string fileformat = SaveFormat)
    {
        string path = SavePath + filePath + fileformat;
        CreateDirectory(path);
        using (FileStream file = File.Create(path))
        {
            BinaryFormatter.Serialize(file, data);
        }
    }

    private static void CreateDirectory(string path)
    {
        string folderPath = Path.GetDirectoryName(path);
        if (folderPath != null && !Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
    }
    public static T FileLoad<T>(string filePath, string fileformat = SaveFormat)
    {
        string path = SavePath + filePath + fileformat;
        CreateDirectory(path);
        if (File.Exists(path))
        {
            using (FileStream file = File.Open(SavePath + filePath + fileformat, FileMode.Open))
            {
                return (T)BinaryFormatter.Deserialize(file);
            }
        }
        // Debug.LogWarning("File " + filePath + " does not exist");
        return default;
    }
    
    public static void SaveScreenShot(string filePath)
    { 
        Texture2D texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        texture.Apply();
        SaveImage(texture, filePath);
    }
    
    public static void SaveImage(Texture2D texture, string filePath)
    {
        string path = SavePath + filePath + ".png";
        CreateDirectory(path);
        File.WriteAllBytes(path, texture.EncodeToPNG());
    }
    
    public static Sprite LoadImage(string filePath)
    {
        string path = SavePath + filePath + ".png";
        CreateDirectory(path);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(File.ReadAllBytes(path));
        return Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f) // Pivot at center
        );
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
