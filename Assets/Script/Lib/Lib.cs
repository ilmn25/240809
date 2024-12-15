using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Lib
{
    public static Vector3 AddToVector(Vector3 vector, float x, float y, float z)
    {
        return new Vector3(vector.x + x, vector.y + y, vector.z + z);
    }

    public static Vector3Int AddToVector(Vector3Int vector, int x, int y, int z)
    {
        return new Vector3Int(vector.x + x, vector.y + y, vector.z + z);
    }

    public static void Log(params object[] parameters)
    {
        if (parameters.Length == 0)
        {
            UnityEngine.Debug.Log("Log");
        }
        else
        {
            string logMessage = string.Join(" | ", parameters);
            UnityEngine.Debug.Log(logMessage);
        }
    }
}
