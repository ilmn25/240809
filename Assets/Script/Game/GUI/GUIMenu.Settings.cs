using UnityEngine;

public partial class GUIMenu
{
    private string RenderSettings()
    {
        return
            "1 > Display\n" +
            "2 > Audio\n" +
            "3 > Gameplay\n" +
            "4 > Keybinds";
    }

    private string RenderDisplay()
    {
        Settings s = Settings.Inst;
        Vector2Int res = Settings.Resolutions[s.ResolutionIndex];
        return
            "1 > Fullscreen: " + (s.Fullscreen ? "On" : "Off") + "\n" +
            "2 > Resolution: " + res.x + "x" + res.y + "\n" +
            "3 > Render Scale: " + Settings.RenderScaleLabels[s.RenderScaleIndex] + "\n" +
            "4 > Max FPS: " + Settings.FpsLimits[s.FpsIndex] + "\n" +
            "5 > Max FOV: " + Settings.MaxFOVs[s.MaxFOVIndex];
    }

    private string RenderAudio()
    {
        Settings s = Settings.Inst;
        return
            "1 > BGM Volume: " + Mathf.RoundToInt(s.BgmVolume * 100) + "\n" +
            "2 > Ambience Volume: " + Mathf.RoundToInt(s.AmbienceVolume * 100) + "\n" +
            "3 > SFX Volume: " + Mathf.RoundToInt(s.SfxVolume * 100);
    }

    private string RenderGameplay()
    {
        Settings s = Settings.Inst;
        return
            "1 > Text Speed: " + (Settings.ScrollSpeeds[s.ScrollSpeedIndex] * 100) + "%\n" +
            "2 > Tutorial: " + (s.TutorialEnabled ? "On" : "Off");
    }

    private void CycleDisplay(int index)
    {
        Settings s = Settings.Inst;
        switch (index)
        {
            case 0:
                s.Fullscreen = !s.Fullscreen;
                break;
            case 1:
                s.ResolutionIndex = (s.ResolutionIndex + 1) % Settings.Resolutions.Length;
                break;
            case 2:
                s.RenderScaleIndex = (s.RenderScaleIndex + 1) % Settings.RenderScales.Length;
                break;
            case 3:
                s.FpsIndex = (s.FpsIndex + 1) % Settings.FpsLimits.Length;
                break;
            case 4:
                s.MaxFOVIndex = (s.MaxFOVIndex + 1) % Settings.MaxFOVs.Length;
                break;
        }
        ApplySettings();
    }

    private void CycleAudio(int index)
    {
        Settings s = Settings.Inst;
        switch (index)
        {
            case 0:
                s.BgmVolume = CycleVolume(s.BgmVolume);
                break;
            case 1:
                s.AmbienceVolume = CycleVolume(s.AmbienceVolume);
                break;
            case 2:
                s.SfxVolume = CycleVolume(s.SfxVolume);
                break;
        }
        ApplySettings();
    }

    private void CycleGameplay(int index)
    {
        Settings s = Settings.Inst;
        switch (index)
        {
            case 0:
                s.ScrollSpeedIndex = (s.ScrollSpeedIndex + 1) % Settings.ScrollSpeeds.Length;
                break;
            case 1:
                s.TutorialEnabled = !s.TutorialEnabled;
                break;
        }
        ApplySettings();
    }

    private void ApplySettings()
    {
        Settings.Apply();
        Settings.Save();
        RenderNoScroll();
    }

    private static float CycleVolume(float v)
    {
        int percent = Mathf.RoundToInt(v * 100);
        percent = (percent + 10) % 110; // 0,10,...,100 -> 0
        return Mathf.Clamp(percent, 0, 100) / 100f;
    }
}
