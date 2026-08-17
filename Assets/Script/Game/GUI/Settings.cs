using System;
using UnityEngine;

[Serializable]
public class Settings
{
    public static Settings Inst;
    public bool Fullscreen = true;
    public int ResolutionIndex = 3;
    public float BgmVolume = 1f;
    public float AmbienceVolume = 1f;
    public float SfxVolume = 0.5f;
    public int ScrollSpeedIndex = 1;
    public bool TutorialEnabled = true;
    public int FpsIndex = 2;
    public int MaxFOVIndex = 2;
    public int AutoSaveIndex = 1; // 0 = off, 1 = every day (default)

    public static readonly float[] ScrollSpeeds = { 0.5f, 1f, 2f, 4f };
    public static readonly int[] FpsLimits = { 30, 60, 100, 144, 240 };
    public static readonly float[] MaxFOVs = { 30f, 40f, 50f, 60f };

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
        Application.targetFrameRate = FpsLimits[Mathf.Clamp(Inst.FpsIndex, 0, FpsLimits.Length - 1)];
    }

    public static void Save()
    {
        Helper.FileSave(Inst, "Settings");
    }
}
