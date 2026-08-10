using System;
using UnityEngine;

[Serializable]
public class Settings
{
    public static Settings Inst;
    public bool Fullscreen = true;
    public int ResolutionIndex = 2;
    public float BgmVolume = 1f;
    public float SfxVolume = 0.5f;
    public int ScrollSpeedIndex = 1;

    public static readonly float[] ScrollSpeeds = { 0.5f, 1f, 2f, 4f };

    public static readonly Vector2Int[] Resolutions =
    {
        new Vector2Int(960, 540),
        new Vector2Int(1280, 720),
        new Vector2Int(1600, 900),
        new Vector2Int(1920, 1080),
    };

    public static void Initialize()
    {
        Inst = Helper.FileLoad<Settings>("Settings") ?? new Settings();
        Apply();
    }

    public static void Apply()
    {
        Vector2Int res = Resolutions[Mathf.Clamp(Inst.ResolutionIndex, 0, Resolutions.Length - 1)];
        Screen.SetResolution(res.x, res.y, Inst.Fullscreen);
    }

    public static void Save()
    {
        Helper.FileSave(Inst, "Settings");
    }
}
