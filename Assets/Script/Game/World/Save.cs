using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;

[Serializable]
public class Save
{
    public static Save Inst; 
    public readonly List<SaveData> List = new();
    public static string TempPath => "RunTime\\";  
    
    public static void Initialize()
    {
        Inst = Helper.FileLoad<Save>("Main");
        if (Inst == null)
        {
            Inst = new(); 
        }  
    }
    public static void Quit()
    {
        Helper.FileSave(Inst, "Main");
    }
    
    public static void NewSave(GenType gen)
    { 
        Helper.DeleteFolder(TempPath);
        Scene.SwitchWorld(new(gen));
    }
    
    public static void CloneSave()
    {
        _ = new CoroutineTask(CloneSaveCoroutine());
        IEnumerator CloneSaveCoroutine()
        {
            yield return new WaitForEndOfFrame();  
            World.UnloadWorld();
            Helper.FileSave(World.Inst, TempPath + SaveData.Inst.current);
            SaveData data = (SaveData)Helper.Clone(SaveData.Inst);
            data.id = DateTime.Now.ToString("yyMMddHHmmss");
            Inst.List.Add(data);
            Helper.CloneFolder(TempPath, data.Path);
            Helper.SaveScreenShot(data.Path + "Preview");
            GUILoad.AddToList(Inst.List.Count - 1); 
            World.LoadWorld(); 
        }
    }
    
    public static void LoadSave(SaveData saveData)
    {   
        SaveData.Inst = (SaveData)Helper.Clone(saveData);
        Helper.CloneFolder(SaveData.Inst.Path, TempPath);
    } 
}

[Serializable]
public class SaveData
{
    public static SaveData Inst;
    public string Path => id + "\\";
    public string id;
    public int day = 1;
    public int time;
    public EnvironmentType weather = EnvironmentType.Sunrise;
    public GenType current;

    public SaveData(){}
    public SaveData(GenType gen)
    {
        current = gen;
    }
}

