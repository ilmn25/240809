using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class Save
{
    public static Save Inst; 
    public readonly Dictionary<string, SaveData> List = new();
    private static string _current = "";
    public static string SavePath => _current + "\\";  
    
    public static void Initialize()
    {
        Inst = Helper.FileLoad<Save>("Main") ?? new();
    }
    public static void Quit()
    {
        Helper.FileSave(Inst, "Main");
    }
    
    public static void NewSave(string id, GenType gen)
    { 
        Inst.List[id] = new()
        {
            current = gen
        };
    }
    
    public static void LoadSave(string id)
    {  
        _current = id; 
        SaveData.Inst = Inst.List[id];
    } 

    public static void SaveSave()
    {
        _ = new CoroutineTask(SaveSave());
        return;
        IEnumerator SaveSave()
        {
            yield return new WaitForEndOfFrame(); 
             
            Helper.SaveScreenShot(SavePath + "Preview");
  
            Scene.UnloadWorld();
        }
    } 
}

[Serializable]
public class SaveData
{
    public static SaveData Inst; 
    public int day = 1;
    public int time;
    public EnvironmentType weather = EnvironmentType.Sunrise;
    public GenType current;
}

